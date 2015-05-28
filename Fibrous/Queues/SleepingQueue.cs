namespace Fibrous
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;

    public sealed class SleepingQueue : IQueue
    {
        private List<Action> Actions = new List<Action>(1024);
        private List<Action> ToPass = new List<Action>(1024);
        private PaddedBoolean _signalled = new PaddedBoolean(false);
        private readonly object _syncRoot = new object();
        private readonly TimeSpan _timeout = TimeSpan.FromMilliseconds(100);

        public void Wait()
        {
            SpinWait spinWait = default(SpinWait);
            Stopwatch sw = Stopwatch.StartNew();
            while (!_signalled.Value) // volatile read
            {
                spinWait.SpinOnce();
                if (sw.Elapsed > _timeout)
                    break;
            }
            _signalled.Exchange(false);
        }

        public void Enqueue(Action action)
        {
            lock (_syncRoot)
            {
                Actions.Add(action);
            }
            _signalled.LazySet(true);
        }

        public List<Action> Drain()
        {
            return DequeueAll();
        }

        public int Count { get { return Actions.Count; } }

        private List<Action> DequeueAll()
        {
            Wait();
            lock (_syncRoot)
            {
                if (Actions.Count == 0) return Queue.Empty;
                Lists.Swap(ref Actions, ref ToPass);
                Actions.Clear();
                return ToPass;
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