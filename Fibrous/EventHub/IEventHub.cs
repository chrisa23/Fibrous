using System;

namespace Fibrous
{

    /// <summary>
    ///   Injectable event hub where classes implementing a combination of
    ///   IHaveFiber / IHandle or IHaveAsyncFiber / IHandleAsync
    ///   can auto wire up to receive events publish on the hub
    /// </summary>
    public interface IEventHub
    {
        IDisposable Subscribe(object handler);
        void Publish<T>(T msg);
    }
}