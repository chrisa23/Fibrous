using System;

namespace Fibrous.Fibers.Thread
{
    public sealed class QueueFullException : Exception
    {
        private readonly int _depth;

        public QueueFullException(int depth)
            : base("Attempted to enqueue item into full queue: " + depth)
        {
            _depth = depth;
        }

        public QueueFullException(string msg)
            : base(msg)
        {
        }

        public int Depth
        {
            get { return _depth; }
        }
    }
}