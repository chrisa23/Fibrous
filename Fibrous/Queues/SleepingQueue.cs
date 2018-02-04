namespace Fibrous.Queues
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using Fibrous.Util;

    public sealed class SleepingQueue : IQueue
    {
        private List<Action> _actions = new List<Action>(1024*32);
        private List<Action> _toPass = new List<Action>(1024*32);
        private PaddedBoolean _signalled = new PaddedBoolean(false);
        private readonly object _syncRoot = new object();
        private readonly TimeSpan _timeout = TimeSpan.FromMilliseconds(100);
        private readonly Stopwatch sw = Stopwatch.StartNew();
        private SpinWait _spinWait = default(SpinWait);
        public void Wait()
        {
            sw.Restart();
            while (!_signalled.Value) // volatile read
            {
                _spinWait.SpinOnce();
                if (sw.Elapsed > _timeout)
                    break;
            }
            _signalled.Exchange(false);
        }

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
            lock (_syncRoot)
            {
                Monitor.PulseAll(_syncRoot);
            }
            _signalled.LazySet(true);
        }
    }
}