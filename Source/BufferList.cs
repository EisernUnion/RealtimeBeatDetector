using System.Collections.Generic;

namespace RealtimeBeatDetector
{
    /// <summary>
    /// A fixed-length buffer list that maintains the most recent N items
    /// </summary>
    /// <typeparam name="T">Type of items in the buffer</typeparam>
    public class BufferList<T> : List<T>
    {
        /// <summary>
        /// Gets or sets the maximum length of the buffer
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// Creates a new BufferList with the specified length
        /// </summary>
        /// <param name="length">Maximum number of items to store</param>
        public BufferList(int length)
        {
            Length = length;
        }

        /// <summary>
        /// Adds an item to the beginning of the buffer and removes the oldest item if the buffer is full
        /// </summary>
        /// <param name="item">Item to add</param>
        public new void Add(T item)
        {
            Insert(0, item);

            if (Count > Length)
            {
                RemoveAt(Count - 1);
            }
        }
    }
}
