namespace Fibrous
{
    using System;

    /// <summary>
    /// Simple subscribable event with Dispose() for unsubscription.  
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    public interface IEvent<TEvent> : IPublisherPort<TEvent>, IDisposable
    {
        IDisposable Subscribe(Action<TEvent> receive);
    }
}