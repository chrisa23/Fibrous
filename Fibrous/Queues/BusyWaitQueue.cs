namespace Fibrous.Queues
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;

    /// <summary>
    /// Busy waits on lock to execute.  Can improve performance in certain situations.
    /// </summary>
    internal sealed class BusyWaitQueue : IQueue
    {
        private readonly object _lock = new object();
        private readonly int _spinsBeforeTimeCheck;
        private readonly int _msBeforeBlockingWait;
        private List<Action> _actions = new List<Action>();
        private List<Action> _toPass = new List<Action>();

        ///<summary>
        /// BusyWaitQueue with custom executor.
        ///</summary>
        ///<param name="spinsBeforeTimeCheck"></param>
        ///<param name="msBeforeBlockingWait"></param>
        public BusyWaitQueue(int spinsBeforeTimeCheck, int msBeforeBlockingWait)
        {
            _spinsBeforeTimeCheck = spinsBeforeTimeCheck;
            _msBeforeBlockingWait = msBeforeBlockingWait;
        }

        /// <summary>
        /// Enqueue action.
        /// </summary>
        /// <param name="action"></param>
        public void Enqueue(Action action)
        {
            lock (_lock)
            {
                _actions.Add(action);
                Monitor.PulseAll(_lock);
            }
        }

        public List<Action> Drain()
        {
            int spins = 0;
            Stopwatch stopwatch = Stopwatch.StartNew();
            while (true)
            {
                try
                {
                    while (!Monitor.TryEnter(_lock))
                    {
                    }
                    List<Action> toReturn = TryDequeue();
                    if (toReturn != null) return toReturn;
                    if (TryBlockingWait(stopwatch, ref spins))
                    {
                        toReturn = TryDequeue();
                        if (toReturn != null) return toReturn;
                    }
                }
                finally
                {
                    Monitor.Exit(_lock);
                }
                Thread.Yield();
                return Queue.Empty;
            }
            
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
            if (_actions.Count > 0)
            {
                Lists.Swap(ref _actions, ref _toPass);
                _actions.Clear();
                return _toPass;
            }
            return null;
        }

        public void Dispose()
        {
        }
    }
}