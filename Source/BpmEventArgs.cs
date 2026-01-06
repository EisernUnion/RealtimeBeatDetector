using System;

namespace RealtimeBeatDetector
{
    /// <summary>
    /// Event arguments containing BPM (beats per minute) information
    /// </summary>
    public class BpmEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the instantaneous BPM from the last beat interval
        /// </summary>
        public double Bpm { get; set; }
        
        /// <summary>
        /// Gets or sets the rolling average BPM
        /// </summary>
        public double AvgBpm { get; set; }

        /// <summary>
        /// Creates a new instance of BpmEventArgs
        /// </summary>
        /// <param name="bpm">The instantaneous BPM</param>
        /// <param name="avgBpm">The average BPM</param>
        public BpmEventArgs(double bpm, double avgBpm = 0)
        {
            Bpm = bpm;
            AvgBpm = avgBpm;
        }
    }
}
