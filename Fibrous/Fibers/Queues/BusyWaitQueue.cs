namespace Fibrous.Fibers.Queues
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;

    public sealed class BusyWaitQueue : QueueBase
    {
        private readonly int _spinsBeforeTimeCheck;
        private readonly int _msBeforeBlockingWait;

        public BusyWaitQueue(IExecutor executor, int spinsBeforeTimeCheck, int msBeforeBlockingWait)
            : base(executor)
        {
            _spinsBeforeTimeCheck = spinsBeforeTimeCheck;
            _msBeforeBlockingWait = msBeforeBlockingWait;
        }

        public BusyWaitQueue(int spinsBeforeTimeCheck, int msBeforeBlockingWait)
            : this(new DefaultExecutor(), spinsBeforeTimeCheck, msBeforeBlockingWait)
        {
        }

        protected override IEnumerable<Action> DequeueAll()
        {
            int spins = 0;
            Stopwatch stopwatch = Stopwatch.StartNew();
            while (true)
            {
                try
                {
                    while (!Monitor.TryEnter(SyncRoot))
                    {
                    }
                    if (!Running)
                    {
                        break;
                    }
                    List<Action> toReturn = TryDequeue();
                    if (toReturn != null)
                    {
                        return toReturn;
                    }
                    if (TryBlockingWait(stopwatch, ref spins))
                    {
                        if (!Running)
                        {
                            break;
                        }
                        toReturn = TryDequeue();
                        if (toReturn != null)
                        {
                            return toReturn;
                        }
                    }
                }
                finally
                {
                    Monitor.Exit(SyncRoot);
                }
                Thread.Yield();
            }
            return null;
        }

        private bool TryBlockingWait(Stopwatch stopwatch, ref int spins)
        {
            if (spins++ < _spinsBeforeTimeCheck)
            {
                return false;
            }
            spins = 0;
            if (stopwatch.ElapsedMilliseconds > _msBeforeBlockingWait)
            {
                Monitor.Wait(SyncRoot);
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
    }
}