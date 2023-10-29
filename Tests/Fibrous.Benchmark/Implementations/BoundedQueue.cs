using System;
using System.Collections.Generic;
using System.Threading;

namespace Fibrous
{
    /// <summary>
    ///     Queue with bounded capacity.  Will throw exception if capacity does not recede prior to wait time.
    ///     Good for putting back pressure on fast publishers
    /// </summary>
    public sealed class BoundedQueue : IQueue
    {
        private readonly object _lock = new();
        private readonly int _maxDepth;
        private readonly int _maxWaitTime;
        private List<Action> _actions = new(1024);
        private List<Action> _toPass = new(1024);

        public BoundedQueue(int depth, int maxWaitTime = int.MaxValue)
        {
            _maxWaitTime = maxWaitTime;
            _maxDepth = depth;
        }

        /// <summary>
        ///     Enqueue action.
        /// </summary>
        /// <param name="action"></param>
        public void Enqueue(Action action)
        {
            lock (_lock)
            {
                if (SpaceAvailable(1))
                {
                    _actions.Add(action);
                    Monitor.PulseAll(_lock);
                }
            }
        }

        public List<Action> Drain()
        {
            lock (_lock)
            {
                if (ReadyToDequeue())
                {
                    Lists.Swap(ref _actions, ref _toPass);
                    _actions.Clear();
                    Monitor.PulseAll(_lock);
                    return _toPass;
                }

                return Queue.Empty;
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                Monitor.PulseAll(_lock);
            }
        }

        private bool SpaceAvailable(int toAdd)
        {
            while (_maxDepth > 0 && _actions.Count + toAdd > _maxDepth)
            {
                if (_maxWaitTime <= 0)
                {
                    throw new QueueFullException(_actions.Count);
                }

                Monitor.Wait(_lock, _maxWaitTime);
                if (_maxDepth > 0 && _actions.Count + toAdd > _maxDepth)
                {
                    throw new QueueFullException(_actions.Count);
                }

                Thread.Sleep(0);
            }

            return true;
        }

        private bool ReadyToDequeue()
        {
            while (_actions.Count == 0)
            {
                Monitor.Wait(_lock);
            }

            return true;
        }
    }
}
