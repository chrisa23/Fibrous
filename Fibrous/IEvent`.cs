using System;

namespace Fibrous
{
    /// <summary>
    ///     Simple subscribe event with Dispose() for unsubscribe.
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    public interface IEvent<TEvent> : IPublisherPort<TEvent>, IDisposable
    {
        IDisposable Subscribe(Action<TEvent> receive);
    }

    public sealed class Event<TEvent> : IEvent<TEvent>
    {
        public IDisposable Subscribe(Action<TEvent> receive)
        {
            InternalEvent += receive;
            return new DisposeAction(() => InternalEvent -= receive);
        }

        public void Publish(TEvent msg)
        {
            var internalEvent = InternalEvent;
            internalEvent?.Invoke(msg);
        }

        public void Dispose()
        {
            InternalEvent = null;
        }

        private event Action<TEvent> InternalEvent;

        internal bool HasSubscriptions => InternalEvent != null;
        
    }
}