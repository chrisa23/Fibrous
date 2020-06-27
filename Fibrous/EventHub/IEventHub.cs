using System;

namespace Fibrous
{

    /// <summary>
    ///   Injectable event hub where classes implementing a combination of
    ///   IHandle or IHandleAsync can auto wire up to receive events publish on the hub.
    ///   
    /// </summary>
    public interface IEventHub
    {
        IDisposable Subscribe(IAsyncFiber fiber, object handler);
        IDisposable Subscribe(IFiber fiber, object handler);
        void Publish<T>(T msg);
    }
}