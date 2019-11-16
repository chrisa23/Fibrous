using System;

namespace Fibrous
{
    public class QueueFullException : Exception
    {
        public QueueFullException(int count)
        {
            Count = count;
        }

        public int Count { get; set; }
    }
}