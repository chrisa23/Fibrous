namespace Fibrous.Fibers.Queues
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// Provides a blocking mechanism around a non-threadsafe queue
    /// </summary>
    public sealed class DefaultQueue : QueueBase
    {
        private readonly object _syncRoot = new object();

        public override void Enqueue(Action action)
        {
            lock (_syncRoot)
            {
                Actions.Add(action);
                Monitor.PulseAll(_syncRoot);
            }
        }

        public override void Drain(IExecutor executor)
        {
            executor.Execute(DequeueAll());
        }

        public IEnumerable<Action> DequeueAll()
        {
            lock (_syncRoot)
            {
                if (Actions.Count == 0)
                {
                    Monitor.Wait(_syncRoot);
                }
                if (Actions.Count == 0) return Queue.Empty;
                Lists.Swap(ref Actions, ref ToPass);
                Actions.Clear();
                return ToPass;
            }
        }

        public override void Dispose()
        {
            lock (_syncRoot)
            {
                Monitor.PulseAll(_syncRoot);
            }
        }
    }
}