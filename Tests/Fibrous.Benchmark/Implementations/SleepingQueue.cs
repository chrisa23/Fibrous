using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Fibrous.Util;

namespace Fibrous
{
    public sealed class SleepingQueue : IQueue
    {
        private readonly Stopwatch _sw = Stopwatch.StartNew();
        private readonly object _syncRoot = new object();
        private readonly TimeSpan _timeout = TimeSpan.FromMilliseconds(100);
        private List<Action> _actions = new List<Action>(1024 * 32);
        private PaddedBoolean _signalled = new PaddedBoolean(false);
        private List<Action> _toPass = new List<Action>(1024 * 32);

        public int Count => _actions.Count;

        public void Enqueue(Action action)
        {
            lock (_syncRoot)
            {
                _actions.Add(action);
            }

            _signalled.LazySet(true);
        }

        public List<Action> Drain() => DequeueAll();

        public void Dispose()
        {
            lock (_syncRoot)
            {
                Monitor.PulseAll(_syncRoot);
            }

            _signalled.LazySet(true);
        }

        public void Wait()
        {
            SpinWait spin = new SpinWait();
            _sw.Restart();
            while (!_signalled.Value) // volatile read
            {
                spin.SpinOnce();
                if (_sw.Elapsed > _timeout)
                {
                    break;
                }
            }

            _signalled.Exchange(false);
        }

        private List<Action> DequeueAll()
        {
            Wait();
            lock (_syncRoot)
            {
                if (_actions.Count == 0)
                {
                    return Queue.Empty;
                }

                Lists.Swap(ref _actions, ref _toPass);
                _actions.Clear();
                return _toPass;
            }
        }
    }
}
