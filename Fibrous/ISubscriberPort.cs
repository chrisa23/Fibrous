namespace Fibrous
{
    using System;

    public interface ISubscriberPort<out T>
    {
        IDisposable Subscribe(IFiber fiber, Action<T> receive);
    }
}