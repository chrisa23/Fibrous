namespace Fibrous.Fibers.Queues
{
    using System;
    using System.Collections.Generic;

    //public class HiPerfQueue : IQueue
    //{
    //    private ActionQueue _internalQueue = new ActionQueue();
    //    private IWaitStrategy _waitStrategy;

    //    public void Dispose()
    //    {
    //        _internalQueue.Dispose();
    //    }

    //    public void Enqueue(Action action)
    //    {
    //        _internalQueue.Enqueue(action);
    //        _waitStrategy.SignalWhenBlocking();
    //    }

    //    public void Drain(IExecutor executor)
    //    {
    //    _internalQueue.Drain();
    //    }

    //    public IEnumerable<Action> DequeueAll()
    //    {
    //        //we need to wait here accoring to wait strategy
    //        if (!_internalQueue.HasItems())
    //        {
    //            _waitStrategy.Wait();
    //        }
    //        return _internalQueue.DequeueAll();
    //    }
    //}
}