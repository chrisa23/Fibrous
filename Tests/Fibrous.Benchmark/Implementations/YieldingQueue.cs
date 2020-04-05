using System;
using System.Collections.Generic;
using System.Threading;
using Fibrous.Util;

namespace Fibrous
{
    public sealed class YieldingQueue : IQueue
    {
        private const int SpinTries = 100;

        private readonly object _syncRoot = new object();
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

        public List<Action> Drain()
        {
            return DequeueAll();
        }

        public void Dispose()
        {
            _signalled.Exchange(true);
        }

        private void Wait()
        {
            var counter = SpinTries;
            while (!_signalled.Value) // volatile read
                ApplyWaitMethod(ref counter);
            _signalled.Exchange(false);
        }

        internal static void ApplyWaitMethod(ref int counter)
        {
            if (counter == 0)
                Thread.Sleep(0);
            else
                --counter;
        }

        internal static int ApplyWaitMethod2(int counter)
        {
            if (counter == 0)
                Thread.Sleep(0);
            else
                --counter;
            return counter;
        }

        private List<Action> DequeueAll()
        {
            Wait();
            lock (_syncRoot)
            {
                if (_actions.Count == 0) return Queue.Empty;
                Lists.Swap(ref _actions, ref _toPass);
                _actions.Clear();
                return _toPass;
            }
        }
    }
}