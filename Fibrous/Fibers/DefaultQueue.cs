namespace Fibrous.Fibers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Fibrous.Utility;

    public sealed class DefaultQueue : QueueBase
    {
        public DefaultQueue(IExecutor executor)
            : base(executor)
        {
        }

        public DefaultQueue()
        {
        }

        protected override IEnumerable<Action> DequeueAll()
        {
            lock (SyncRoot)
            {
                if (ReadyToDequeue())
                {
                    Lists.Swap(ref Actions, ref ToPass);
                    Actions.Clear();
                    return ToPass;
                }
                return null;
            }
        }

        private bool ReadyToDequeue()
        {
            while (Actions.Count == 0 && Running)
            {
                Monitor.Wait(SyncRoot);
            }
            return Running;
        }
    }
}