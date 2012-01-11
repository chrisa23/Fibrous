using System;
using System.Collections.Generic;
using System.Threading;
using Fibrous.Internal;

namespace Fibrous.Fibers.Thread
{
    public sealed class BoundedQueue : IQueue
    {
        private readonly object _lock = new object();
        private readonly IExecutor _executor;

        private bool _running = true;
        private int _maxQueueDepth = -1;
        private int _maxEnqueueWaitTime;

        private List<Action> _actions = new List<Action>();
        private List<Action> _toPass = new List<Action>();


        public BoundedQueue(IExecutor executor)
        {
            _executor = executor;
        }


        public BoundedQueue()
            : this(new DefaultExecutor())
        {
        }


        public int MaxDepth
        {
            get { return _maxQueueDepth; }
            set { _maxQueueDepth = value; }
        }


        public int MaxEnqueueWaitTime
        {
            get { return _maxEnqueueWaitTime; }
            set { _maxEnqueueWaitTime = value; }
        }


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


        public void Run()
        {
            while (ExecuteNextBatch())
            {
            }
        }


        public void Stop()
        {
            lock (_lock)
            {
                _running = false;
                Monitor.PulseAll(_lock);
            }
        }

        private bool SpaceAvailable(int toAdd)
        {
            if (!_running)
            {
                return false;
            }
            while (_maxQueueDepth > 0 && _actions.Count + toAdd > _maxQueueDepth)
            {
                if (_maxEnqueueWaitTime <= 0)
                {
                    throw new QueueFullException(_actions.Count);
                }
                Monitor.Wait(_lock, _maxEnqueueWaitTime);
                if (!_running)
                {
                    return false;
                }
                if (_maxQueueDepth > 0 && _actions.Count + toAdd > _maxQueueDepth)
                {
                    throw new QueueFullException(_actions.Count);
                }
            }
            return true;
        }

        private IEnumerable<Action> DequeueAll()
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
                return null;
            }
        }

        private bool ReadyToDequeue()
        {
            while (_actions.Count == 0 && _running)
            {
                Monitor.Wait(_lock);
            }
            if (!_running)
            {
                return false;
            }
            return true;
        }


        private bool ExecuteNextBatch()
        {
            IEnumerable<Action> toExecute = DequeueAll();
            if (toExecute == null)
            {
                return false;
            }
            _executor.Execute(toExecute);
            return true;
        }
    }
}