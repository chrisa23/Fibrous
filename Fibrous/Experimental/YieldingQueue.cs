namespace Fibrous.Experimental
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Fibrous.Queues;

    public sealed class YieldingQueue : QueueBase
    {
        private const int SpinTries = 100;
        private PaddedBoolean _signalled = new PaddedBoolean(false);

        public void Wait()
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

        public override void Enqueue(Action action)
        {
            lock (_syncRoot)
            {
                Actions.Add(action);
            }
            _signalled.Exchange(true);
        }

        public override void Drain(Executor executor)
        {
            executor.Execute(DequeueAll());
        }

        private IEnumerable<Action> DequeueAll()
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

        public override void Dispose()
        {
            _signalled.Exchange(true);
        }
    }
}