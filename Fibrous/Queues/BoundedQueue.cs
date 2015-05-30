namespace Fibrous.Queues
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    //Note:  Just for exploring/testing, will be significantly changed 

    /// <summary>
    /// Queue with bounded capacity.  Will throw exception if capacity does not recede prior to wait time.
    /// </summary>
    internal sealed class BoundedQueue : IQueue
    {
        private readonly object _lock = new object();
        private List<Action> _actions = new List<Action>(1024);
        private List<Action> _toPass = new List<Action>(1024);

        public BoundedQueue(int depth)
        {
            MaxDepth = depth;
        }

        /// <summary>
        /// Max number of actions to be queued.
        /// </summary>
        public int MaxDepth { get; set; }
        /// <summary>
        /// Max time to wait for space in the queue.
        /// </summary>
        public int MaxEnqueueWaitTimeInMs { get; set; }

        /// <summary>
        /// Enqueue action.
        /// </summary>
        /// <param name="action"></param>
        public void Enqueue(Action action)
        {
            lock (_lock)
            {
                if (SpaceAvailable(1))
                {
                    _actions.Add(action);
                    Monitor.PulseAll(_lock);
                }
            }
        }

        private bool SpaceAvailable(int toAdd)
        {
            while (MaxDepth > 0 && _actions.Count + toAdd > MaxDepth)
            {
                //Monitor.Wait(_lock, 1);
                //hread.Yield(); //??
                //switch to some other mechanism 
                //                if (MaxEnqueueWaitTimeInMs <= 0)
                //                {
                ////                    throw new QueueFullException(_actions.Count);
                //                    return false;
                //                }
                //                Monitor.Wait(_lock, MaxEnqueueWaitTimeInMs);
                //                if (MaxDepth > 0 && _actions.Count + toAdd > MaxDepth)
                //                {
                //                    //throw new QueueFullException(_actions.Count);
                //                    return false;
                //                }
            }
            return true;
        }

        public List<Action> Drain()
        {
            lock (_lock)
            {
                if (ReadyToDequeue())
                {
                    Lists.Swap(ref _actions, ref _toPass);
                    _actions.Clear();
                    Monitor.PulseAll(_lock);
                    return _toPass;
                }
                return Queue.Empty;
            }
        }

        private bool ReadyToDequeue()
        {
            while (_actions.Count == 0)
                Monitor.Wait(_lock);
            return true;
        }

        public void Dispose()
        {
            //Monitor.PulseAll(_lock);
        }
    }
}