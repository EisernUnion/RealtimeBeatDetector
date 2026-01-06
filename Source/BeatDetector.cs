using System;

namespace RealtimeBeatDetector
{
    /// <summary>
    /// Core beat detection algorithm using energy-based detection with adaptive thresholds
    /// </summary>
    public class BeatDetector
    {
        /// <summary>
        /// Event fired when a beat is detected
        /// </summary>
        public event EventHandler BeatDetected;
        
        /// <summary>
        /// Event fired when BPM is calculated
        /// </summary>
        public event EventHandler<BpmEventArgs> BpmDetected;

        /// <summary>
        /// Length of the energy buffer
        /// </summary>
        public const int EnergyBuffLen = 100;
        
        /// <summary>
        /// Width of the averaging window
        /// </summary>
        public const int AvgWindowWidth = 40;
        
        /// <summary>
        /// Minimum spacing between beats in milliseconds
        /// </summary>
        public const int BeatMinSpacingMillis = 400;

        /// <summary>
        /// Buffer storing recent energy values
        /// </summary>
        public BufferList<float> energyBuffer = new BufferList<float>(EnergyBuffLen);
        
        /// <summary>
        /// Buffer storing recent BPM values
        /// </summary>
        public BufferList<float> bpmBuffer = new BufferList<float>(4);
        
        /// <summary>
        /// Buffer for local average calculations
        /// </summary>
        public float[] localAvgBuffer = new float[EnergyBuffLen];
        
        /// <summary>
        /// Buffer for local difference calculations
        /// </summary>
        public float[] localDiffBuffer = new float[EnergyBuffLen];

        private DateTime lastBeat = DateTime.Now;

        /// <summary>
        /// Gets or sets the beat detection threshold. Higher values make detection less sensitive.
        /// </summary>
        public float BeatThreshold { get; set; } = 10f;

        /// <summary>
        /// Process an audio buffer for beat detection
        /// </summary>
        /// <param name="buffer">PCM audio data buffer</param>
        /// <param name="length">Length of data to process</param>
        public void ProcessBuffer(byte[] buffer, int length)
        {
            float[] samples = PCMUtils.PCM32ToSamples(buffer, length);
            float energy = 0;

            for (int i = 0; i < samples.Length; i++)
            {
                float s = samples[i];
                energy += s * s;
            }

            energyBuffer.Add(energy);

            if (energyBuffer.Count == EnergyBuffLen)
            {
                ComputeLocalDiff();
            }
        }

        private void ComputeLocalDiff()
        {
            float max = 0;

            for (int i = 0; i < EnergyBuffLen - AvgWindowWidth; i++)
            {
                float avg = GetAveragePastEnergy(i, AvgWindowWidth);
                float avg2 = GetAveragePastEnergy(i, 3);
                float diff = Math.Max(0, avg2 - avg);

                if (diff > max)
                {
                    max = diff;
                }

                localAvgBuffer[i] = avg;
                localDiffBuffer[i] = diff;
            }


            if (localDiffBuffer[0] > BeatThreshold)
            {
                DetectBeat();
            }

            max /= 3f;
            float avgDiff = GetAveragePastDiff(0, (EnergyBuffLen - AvgWindowWidth) / 2);
            float targetThreshold = Math.Max(1f, Math.Max(avgDiff * 1.5f, max * 0.6f));
            BeatThreshold += (targetThreshold - BeatThreshold) * 0.25f;
        }

        private float GetAveragePastEnergy(int index, int radius)
        {
            float avg = 0;

            for (int j = 0; j <= radius; j++)
            {
                avg += energyBuffer[index + j];
            }

            return avg / (radius + 1);
        }

        private float GetAveragePastDiff(int start, int end)
        {
            float avg = 0;

            for (int j = start; j < end; j++)
            {
                avg += localDiffBuffer[j];
            }

            return avg / (end - start);
        }

        private float GetPastDiff(int start)
        {
            float diff = 0;

            for (int j = start; j < EnergyBuffLen - AvgWindowWidth - 1; j++)
            {
                diff += Math.Abs(localDiffBuffer[j] - localDiffBuffer[j + 1]);
            }

            return diff;
        }

        private void DetectBeat()
        {
            DateTime now = DateTime.Now;
            double millis = now.Subtract(lastBeat).TotalMilliseconds;

            if (millis > BeatMinSpacingMillis)
            {
                BeatDetected?.Invoke(this, null);

                float bpm = (float)(60 / millis * 1000);

                bpmBuffer.Add(bpm);

                float avgBpm = 0;

                for (int i = 0; i < bpmBuffer.Count; i++)
                {
                    avgBpm += bpmBuffer[i];
                }

                avgBpm /= bpmBuffer.Count;

                BpmDetected?.Invoke(this, new BpmEventArgs(bpm, avgBpm));

                lastBeat = now;
            }
        }
    }
}
