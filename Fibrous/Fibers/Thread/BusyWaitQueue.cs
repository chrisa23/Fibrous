using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Fibrous.Internal;

namespace Fibrous.Fibers.Thread
{
    public sealed class BusyWaitQueue : IQueue
    {
        private readonly object _lock = new object();
        private readonly IExecutor _executor;
        private readonly int _spinsBeforeTimeCheck;
        private readonly int _msBeforeBlockingWait;

        private bool _running = true;

        private List<Action> _actions = new List<Action>();
        private List<Action> _toPass = new List<Action>();


        public BusyWaitQueue(IExecutor executor, int spinsBeforeTimeCheck, int msBeforeBlockingWait)
        {
            _executor = executor;
            _spinsBeforeTimeCheck = spinsBeforeTimeCheck;
            _msBeforeBlockingWait = msBeforeBlockingWait;
        }


        public BusyWaitQueue(int spinsBeforeTimeCheck, int msBeforeBlockingWait)
            : this(new DefaultExecutor(), spinsBeforeTimeCheck, msBeforeBlockingWait)
        {
        }


        public void Enqueue(Action action)
        {
            lock (_lock)
            {
                _actions.Add(action);
                Monitor.PulseAll(_lock);
            }
        }


        public void Run()
        {
            while (ExecuteNextBatch())
            {
            }
        }


        public void Stop()
        {
            lock (_lock)
            {
                _running = false;
                Monitor.PulseAll(_lock);
            }
        }

        private IEnumerable<Action> DequeueAll()
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

                    if (!_running) break;
                    List<Action> toReturn = TryDequeue();
                    if (toReturn != null) return toReturn;

                    if (TryBlockingWait(stopwatch, ref spins))
                    {
                        if (!_running) break;
                        toReturn = TryDequeue();
                        if (toReturn != null) return toReturn;
                    }
                }
                finally
                {
                    Monitor.Exit(_lock);
                }
                System.Threading.Thread.Yield();
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

        private bool ExecuteNextBatch()
        {
            IEnumerable<Action> toExecute = DequeueAll();
            if (toExecute == null)
            {
                return false;
            }
            _executor.Execute(toExecute);
            return true;
        }
    }
}