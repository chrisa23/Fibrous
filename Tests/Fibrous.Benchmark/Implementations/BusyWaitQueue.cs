using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Fibrous
{
    /// <summary>
    ///     Busy waits on lock to execute.  Can improve performance in certain situations.
    /// </summary>
    public sealed class BusyWaitQueue : IQueue
    {
        private readonly object _lock = new();
        private readonly int _msBeforeBlockingWait;
        private readonly int _spinsBeforeTimeCheck;
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
        private List<Action> _actions = new(1024 * 32);
        private List<Action> _toPass = new(1024 * 32);

        /// <summary>
        ///     BusyWaitQueue with custom executor.
        /// </summary>
        /// <param name="spinsBeforeTimeCheck"></param>
        /// <param name="msBeforeBlockingWait"></param>
        public BusyWaitQueue(int spinsBeforeTimeCheck, int msBeforeBlockingWait)
        {
            _spinsBeforeTimeCheck = spinsBeforeTimeCheck;
            _msBeforeBlockingWait = msBeforeBlockingWait;
        }

        /// <summary>
        ///     Enqueue action.
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
            _stopwatch.Restart();
            while (true)
            {
                try
                {
                    while (!Monitor.TryEnter(_lock))
                    {
                    }

                    List<Action> toReturn = TryDequeue();
                    if (toReturn != null)
                    {
                        return toReturn;
                    }

                    if (TryBlockingWait(_stopwatch, ref spins))
                    {
                        toReturn = TryDequeue();
                        if (toReturn != null)
                        {
                            return toReturn;
                        }
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

        public void Dispose()
        {
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
    }
}
