namespace Fibrous.Queues
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;

    public sealed class DefaultQueue : IQueue
    {
        protected List<Action> Actions = new List<Action>();
        protected List<Action> ToPass = new List<Action>();

        private readonly int _msBeforeBlockingWait;
        private readonly int _spinsBeforeTimeCheck;
        private readonly object _lock = new object();

        public DefaultQueue(int spinsBeforeTimeCheck, int msBeforeBlockingWait)
        {
            _spinsBeforeTimeCheck = spinsBeforeTimeCheck;
            _msBeforeBlockingWait = msBeforeBlockingWait;
        }

        public DefaultQueue()
            : this(100, 10)
        {
            
        }

        public void Enqueue(Action action)
        {
            lock (_lock)
            {
                Actions.Add(action);
                Monitor.PulseAll(_lock);
            }
        }

        public IEnumerable<Action> DequeueAll()
        {
            int spins = 0;
            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                while (!Monitor.TryEnter(_lock))
                {
                }
                List<Action> toReturn = TryDequeue();
                if (toReturn != null)
                    return toReturn;
                if (TryBlockingWait(stopwatch, ref spins))
                {
                    toReturn = TryDequeue();
                    if (toReturn != null)
                        return toReturn;
                }
            }
            finally
            {
                Monitor.Exit(_lock);
            }
            return Queue.Empty;
        }

        private bool TryBlockingWait(Stopwatch stopwatch, ref int spins)
        {
            if (spins++ < _spinsBeforeTimeCheck)
                return false;
            spins = 0;
            if (stopwatch.ElapsedMilliseconds > _msBeforeBlockingWait)
            {
                Monitor.Wait(_lock);
                stopwatch.Restart();
                return true;
            }
            return false;
        }

        private List<Action> TryDequeue()
        {
            if (Actions.Count > 0)
            {
                Lists.Swap(ref Actions, ref ToPass);
                Actions.Clear();
                return ToPass;
            }
            return null;
        }

        public void Drain(Executor executor)
        {
            IEnumerable<Action> dequeueAll = DequeueAll();
            executor.Execute(dequeueAll);
        }

        public void Dispose()
        {
        
        }
    }
}