using System;

namespace Fibrous
{
    public class QueueFullException : Exception
    {
        public int Count { get; set; }

        public QueueFullException(int count)
        {
            Count = count;
        }
    }
}