namespace Fibrous.Fibers.Queues
{
    using System;
    using System.Collections.Generic;

    public sealed class DefaultQueue : QueueBase
    {
        public override IEnumerable<Action> DequeueAll()
        {
            if (!HasItems()) return Empty;
            lock (SyncRoot)
            {
                Lists.Swap(ref Actions, ref ToPass);
                Actions.Clear();
                return ToPass;
            }
        }
    }
}