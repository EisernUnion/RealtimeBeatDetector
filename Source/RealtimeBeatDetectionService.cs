using System;
using CSCore.SoundIn;

namespace RealtimeBeatDetector
{
    /// <summary>
    /// Service that wraps BeatDetector with audio capture functionality for realtime beat detection.
    /// Uses WasapiLoopbackCapture to capture system audio output.
    /// </summary>
    public class RealtimeBeatDetectionService : IDisposable
    {
        private readonly BeatDetector _beatDetector;
        private WasapiLoopbackCapture _wasapiCapture;
        private bool _disposed;

        /// <summary>
        /// Event fired when a beat is detected
        /// </summary>
        public event EventHandler BeatDetected;

        /// <summary>
        /// Event fired when BPM is calculated
        /// </summary>
        public event EventHandler<BpmEventArgs> BpmDetected;

        /// <summary>
        /// Gets whether the service is currently running
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Gets or sets the beat detection threshold. Higher values make detection less sensitive.
        /// Default is 10f.
        /// </summary>
        public float BeatThreshold
        {
            get => _beatDetector.BeatThreshold;
            set => _beatDetector.BeatThreshold = value;
        }

        /// <summary>
        /// Creates a new instance of RealtimeBeatDetectionService
        /// </summary>
        public RealtimeBeatDetectionService()
        {
            _beatDetector = new BeatDetector();
            _beatDetector.BeatDetected += OnBeatDetected;
            _beatDetector.BpmDetected += OnBpmDetected;
        }

        /// <summary>
        /// Starts capturing audio and detecting beats
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if already running or if WASAPI initialization fails</exception>
        public void Start()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(RealtimeBeatDetectionService));

            if (IsRunning)
                throw new InvalidOperationException("Service is already running");

            try
            {
                _wasapiCapture = new WasapiLoopbackCapture();
                _wasapiCapture.DataAvailable += OnDataAvailable;
                _wasapiCapture.Initialize();
                _wasapiCapture.Start();
                IsRunning = true;
            }
            catch (Exception ex)
            {
                _wasapiCapture?.Dispose();
                _wasapiCapture = null;
                throw new InvalidOperationException("Failed to initialize WASAPI audio capture. Ensure a default playback device is set.", ex);
            }
        }

        /// <summary>
        /// Stops capturing audio and detecting beats
        /// </summary>
        public void Stop()
        {
            if (!IsRunning)
                return;

            try
            {
                if (_wasapiCapture != null)
                {
                    _wasapiCapture.DataAvailable -= OnDataAvailable;
                    _wasapiCapture.Stop();
                    _wasapiCapture.Dispose();
                    _wasapiCapture = null;
                }
            }
            finally
            {
                IsRunning = false;
            }
        }

        private void OnDataAvailable(object sender, DataAvailableEventArgs e)
        {
            _beatDetector.ProcessBuffer(e.Data, e.ByteCount);
        }

        private void OnBeatDetected(object sender, EventArgs e)
        {
            BeatDetected?.Invoke(this, e);
        }

        private void OnBpmDetected(object sender, BpmEventArgs e)
        {
            BpmDetected?.Invoke(this, e);
        }

        /// <summary>
        /// Disposes the service and releases all resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the service and releases all resources
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                Stop();

                if (_beatDetector != null)
                {
                    _beatDetector.BeatDetected -= OnBeatDetected;
                    _beatDetector.BpmDetected -= OnBpmDetected;
                }
            }

            _disposed = true;
        }
    }
}
