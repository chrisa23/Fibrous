using System;
using System.Collections.Generic;
using System.Threading;
using Fibrous.Internal;

namespace Fibrous.Fibers.Thread
{
    public sealed class DefaultQueue : IQueue
    {
        private readonly object _lock = new object();
        private readonly IExecutor _executor;

        private bool _running = true;

        private List<Action> _actions = new List<Action>();
        private List<Action> _toPass = new List<Action>();


        public DefaultQueue(IExecutor executor)
        {
            _executor = executor;
        }


        public DefaultQueue()
            : this(new DefaultExecutor())
        {
        }


        public void Enqueue(Action action)
        {
            lock (_lock)
            {
                _actions.Add(action);
                Monitor.PulseAll(_lock);
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

        private IEnumerable<Action> DequeueAll()
        {
            lock (_lock)
            {
                if (ReadyToDequeue())
                {
                    Lists.Swap(ref _actions, ref _toPass);
                    _actions.Clear();
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
            return _running;
        }


        public bool ExecuteNextBatch()
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