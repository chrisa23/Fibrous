namespace Fibrous.Fibers.Queues
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;

    /// <summary>
    /// Provides a blocking mechanism around a non-threadsafe queue
    /// </summary>
    public sealed class DefaultQueue : QueueBase
    {
        private readonly object _syncRoot = new object();

        public override void Enqueue(Action action)
        {
            lock (_syncRoot)
            {
                Actions.Add(action);
                Monitor.PulseAll(_syncRoot);
            }
        }

        public override IEnumerable<Action> DequeueAll()
        {
            lock (_syncRoot)
            {
                if (Actions.Count == 0) return Empty;
                Lists.Swap(ref Actions, ref ToPass);
                Actions.Clear();
                return ToPass;
            }
        }

        public override void Dispose()
        {
            lock (_syncRoot)
            {
                Monitor.PulseAll(_syncRoot);
            }
        }
    }

    public sealed class YieldingQueue : QueueBase
    {
        private const int SpinTries = 100;
        private PaddedBoolean _signalled = new PaddedBoolean(false);

        public void Wait()
        {
            int counter = SpinTries;
            while (!_signalled.ReadFullFence()) // volatile read
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

        public override void Enqueue(Action action)
        {
            lock (_syncRoot)
            {
                Actions.Add(action);
                Monitor.PulseAll(_syncRoot);
            }
            _signalled.Exchange(true);
        }

        public override IEnumerable<Action> DequeueAll()
        {
            Wait();
            //if (Actions.Count == 0) return Empty;
            lock (_syncRoot)
            {
                Lists.Swap(ref Actions, ref ToPass);
                Actions.Clear();
                return ToPass;
            }
        }

        public override void Dispose()
        {
            _signalled.Exchange(true);
        }
    }

    public sealed class SleepingQueue : QueueBase
    {
        private const int SpinTries = 100;
        private PaddedBoolean _signalled = new PaddedBoolean(false);

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

        private readonly object _syncRoot = new object();
        private readonly TimeSpan _timeout = TimeSpan.FromMilliseconds(100);

        public override void Enqueue(Action action)
        {
            lock (_syncRoot)
            {
                Actions.Add(action);
            }
            _signalled.Exchange(true);
        }

        public override IEnumerable<Action> DequeueAll()
        {
            Wait();
            lock (_syncRoot)
            {
                if (Actions.Count == 0) return Empty;
                Lists.Swap(ref Actions, ref ToPass);
                Actions.Clear();
                return ToPass;
            }
        }

        public override void Dispose()
        {
            lock (_syncRoot)
            {
                Monitor.PulseAll(_syncRoot);
            }
            _signalled.LazySet(true);
        }
    }
}