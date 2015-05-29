namespace Fibrous
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    public sealed class YieldingQueue : IQueue
    {
        private List<Action> Actions = new List<Action>(1024);
        private List<Action> ToPass = new List<Action>(1024);
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
            _signalled.Exchange(true);
        }
    }
}