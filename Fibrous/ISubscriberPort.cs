using System;

namespace Fibrous
{
    public interface ISubscriberPort<out T>
    {
        IDisposable Subscribe(IFiber fiber, Action<T> receive);
    }
}