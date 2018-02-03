namespace Fibrous.Queues
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Fibrous.Util;

    public sealed class YieldingQueue : IQueue
    {
        private List<Action> _actions = new List<Action>(1024);
        private List<Action> _toPass = new List<Action>(1024);
        private const int SpinTries = 100;
        private PaddedBoolean _signalled = new PaddedBoolean(false);

        private void Wait()
        {
            int counter = SpinTries;
            while (!_signalled.Value) // volatile read
                counter = ApplyWaitMethod(counter);
            _signalled.Exchange(false);
        }

        private static int ApplyWaitMethod(int counter)
        {
            if (counter == 0)
                Thread.Sleep(0);
            else
                --counter;
            return counter;
        }

        private readonly object _syncRoot = new object();

        public void Enqueue(Action action)
        {
            lock (_syncRoot)
            {
                _actions.Add(action);
            }
            _signalled.LazySet(true);
        }

        public List<Action> Drain()
        {
            return DequeueAll();
        }

        public int Count => _actions.Count;

        private List<Action> DequeueAll()
        {
            Wait();
            lock (_syncRoot)
            {
                if (_actions.Count == 0) return Queue.Empty;
                Lists.Swap(ref _actions, ref _toPass);
                _actions.Clear();
                return _toPass;
            }
        }

        public void Dispose()
        {
            _signalled.Exchange(true);
        }
    }
}