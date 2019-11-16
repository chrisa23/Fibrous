using System;
using System.Collections.Generic;
using System.Threading;

namespace Fibrous
{
    public class SpinLockQueue : IQueue
    {
        private List<Action> _actions = new List<Action>(1024 * 32);
        private SpinLock _lock = new SpinLock(false);
        private List<Action> _toPass = new List<Action>(1024 * 32);

        public int Count => _actions.Count;


        public void Enqueue(Action action)
        {
            var lockTaken = false;
            try
            {
                _lock.Enter(ref lockTaken);
                _actions.Add(action);
            }
            finally
            {
                if (lockTaken) _lock.Exit();
            }
        }

        public List<Action> Drain()
        {
            return DequeueAll();
        }

        public void Dispose()
        {
        }

        private List<Action> DequeueAll()
        {
            var lockTaken = false;
            try
            {
                _lock.Enter(ref lockTaken);
                if (_actions.Count == 0) return Queue.Empty;
                Lists.Swap(ref _actions, ref _toPass);
                _actions.Clear();
                return _toPass;
            }
            finally
            {
                if (lockTaken) _lock.Exit();
            }
        }
    }
}