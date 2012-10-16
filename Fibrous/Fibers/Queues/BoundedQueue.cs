using System;
using System.Collections.Generic;
using System.Threading;

namespace Fibrous.Fibers.Queues
{
    public sealed class BoundedQueue : QueueBase
    {
        private readonly int _maxEnqueueWaitTime;
        private readonly int _maxQueueDepth = -1;

        public BoundedQueue(IExecutor executor, int maxQueueDepth, int maxEnqueueWaitTime)
            : base(executor)
        {
            _maxQueueDepth = maxQueueDepth;
            _maxEnqueueWaitTime = maxEnqueueWaitTime;
        }

        public BoundedQueue(int maxQueueDepth, int maxEnqueueWaitTime)
        {
            _maxQueueDepth = maxQueueDepth;
            _maxEnqueueWaitTime = maxEnqueueWaitTime;
        }

        public override void Enqueue(Action action)
        {
            lock (SyncRoot)
            {
                if (SpaceAvailable(1))
                {
                    Actions.Add(action);
                    Monitor.PulseAll(SyncRoot);
                }
            }
        }

        private bool SpaceAvailable(int toAdd)
        {
            if (!Running)
                return false;
            while (_maxQueueDepth > 0 && Actions.Count + toAdd > _maxQueueDepth)
            {
                if (_maxEnqueueWaitTime <= 0)
                    throw new QueueFullException(Actions.Count);
                Monitor.Wait(SyncRoot, _maxEnqueueWaitTime);
                if (!Running)
                    return false;
                if (_maxQueueDepth > 0 && Actions.Count + toAdd > _maxQueueDepth)
                    throw new QueueFullException(Actions.Count);
            }
            return true;
        }

        protected override IEnumerable<Action> DequeueAll()
        {
            lock (SyncRoot)
            {
                if (ReadyToDequeue())
                {
                    Lists.Swap(ref Actions, ref ToPass);
                    Actions.Clear();
                    Monitor.PulseAll(SyncRoot);
                    return ToPass;
                }
                return null;
            }
        }

        private bool ReadyToDequeue()
        {
            while (Actions.Count == 0 && Running)
                Monitor.Wait(SyncRoot);
            if (!Running)
                return false;
            return true;
        }
    }
}