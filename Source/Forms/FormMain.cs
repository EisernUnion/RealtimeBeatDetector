using CSCore.SoundIn;
using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RealtimeBeatDetector.Source.Forms
{
    public partial class FormMain : Form
    {
        private readonly BeatDetector beatDetector;
        private readonly WasapiLoopbackCapture wasapi;
        private readonly HttpClient httpClient;
        private const string UrlConfigFile = "api_url.txt";
        private readonly string apiUrl;

        public FormMain()
        {
            InitializeComponent();
            Application.ApplicationExit += Application_ApplicationExit;

            httpClient = new HttpClient();
            apiUrl = ReadApiUrlFromFile();

            // Initialize BeatDetector
            beatDetector = new BeatDetector();
            beatDetector.BeatDetected += BeatDetector_BeatDetected;
            beatDetector.BpmDetected += BeatDetector_BpmDetected;

            // Initialize WASAPI
            try
            {
                wasapi = new WasapiLoopbackCapture();
                wasapi.DataAvailable += Wasapi_DataAvailable;
                wasapi.Initialize();
                wasapi.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n\nThe application will now exit.", "WASAPI error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
                return;
            }

            Timer t = new Timer();
            t.Tick += T_Tick;
            t.Interval = 16;
            t.Start();
        }

        private async void BeatDetector_BpmDetected(object sender, BpmEventArgs e)
        {
            if (!IsDisposed && !Disposing)
            {
                bpmCounter.Invoke(new Action(() =>
                {
                    bpmCounter.Text = e.AvgBpm.ToString("#");
                }));

                await CallBeatApiAsync();
            }
        }

        private async Task CallBeatApiAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(apiUrl))
                {
                    return;
                }

                var content = new StringContent("1.0", Encoding.UTF8, "application/json");
                await httpClient.PostAsync(apiUrl, content);
            }
            catch (Exception)
            {
                // Silently ignore API errors to prevent disrupting beat detection
            }
        }

        private string ReadApiUrlFromFile()
        {
            try
            {
                if (File.Exists(UrlConfigFile))
                {
                    string url = File.ReadAllText(UrlConfigFile).Trim();
                    return url;
                }
                else
                {
                    // Create default config file if it doesn't exist
                    File.WriteAllText(UrlConfigFile, "https://localhost:44339/api/beat");
                    return " ";
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void T_Tick(object sender, EventArgs e)
        {
            BackColor = Color.FromArgb(255, val, val, val);
            Invalidate();

            val = Math.Max(0, val - 40);
        }

        private int val = 0;

        private void BeatDetector_BeatDetected(object sender, EventArgs e)
        {
            val = 255;
        }

        private void Wasapi_DataAvailable(object sender, DataAvailableEventArgs e)
        {
            beatDetector.ProcessBuffer(e.Data, e.ByteCount);
        }

        private void Application_ApplicationExit(object sender, EventArgs e)
        {
            beatDetector.BeatDetected -= BeatDetector_BeatDetected;
            beatDetector.BpmDetected -= BeatDetector_BpmDetected;

            wasapi.DataAvailable -= Wasapi_DataAvailable;
            wasapi.Stop();
            wasapi.Dispose();

            httpClient?.Dispose();
        }
    }
}
