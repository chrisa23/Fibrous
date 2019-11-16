namespace Fibrous
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// Blocking implementation.
    /// </summary>
    public class BlockingQueue : IQueue
    {
        private readonly object _lock = new object();
        private List<Action> _actions = new List<Action>(1024);
        private List<Action> _toPass = new List<Action>(1024);

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
            lock (_lock)
            {
                if (ReadyToDequeue())
                {
                    Lists.Swap(ref _actions, ref _toPass);
                    _actions.Clear();
                    return _toPass;
                }
                return null;
            }
        }

        private bool ReadyToDequeue()
        {
            while (_actions.Count == 0)
            {
                Monitor.Wait(_lock);
            }
            return true;
        }

        public void Dispose()
        {
            lock (_lock)
            {
                Monitor.PulseAll(_lock);
            }
        }
    }
}