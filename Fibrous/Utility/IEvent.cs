namespace Fibrous.Utility
{
    using System;

    public interface IEvent<out TEvent>
    {
        IDisposable Subscribe(Action<TEvent> receive);
    }
}