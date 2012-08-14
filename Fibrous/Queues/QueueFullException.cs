namespace Fibrous.Queues
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
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

        private QueueFullException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public int Depth { get { return _depth; } }
    }
}