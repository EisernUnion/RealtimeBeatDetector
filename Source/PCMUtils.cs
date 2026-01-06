using System;

namespace RealtimeBeatDetector
{
    /// <summary>
    /// Utility class for PCM audio data processing
    /// </summary>
    public abstract class PCMUtils
    {
        /// <summary>
        /// Converts PCM 32-bit float audio data to mono samples
        /// </summary>
        /// <param name="buffer">Buffer containing PCM audio data</param>
        /// <param name="length">Length of data to process</param>
        /// <returns>Array of mono audio samples</returns>
        public static float[] PCM32ToSamples(byte[] buffer, int length)
        {
            float[] samples = new float[length / sizeof(float)];
            Buffer.BlockCopy(buffer, 0, samples, 0, length);

            float[] result = new float[length / sizeof(float) / 2];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = MergeSamples(samples, i * 2, 2);
            }

            return result;
        }

        /// <summary>
        /// Merges multiple audio channels into a single mono sample by averaging
        /// </summary>
        /// <param name="samples">Array of samples</param>
        /// <param name="index">Starting index</param>
        /// <param name="channelCount">Number of channels to merge</param>
        /// <returns>Merged mono sample</returns>
        public static float MergeSamples(float[] samples, int index, int channelCount)
        {
            float z = 0f;
            for (int i = 0; i < channelCount; i++)
            {
                z += samples[index + i];
            }
            return z / channelCount;
        }
    }
}
