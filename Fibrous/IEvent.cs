using System;
using System.Runtime.CompilerServices;

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

        internal bool HasSubscriptions()
        {
            return InternalEvent != null;
        }
    }

    public sealed class Event : IDisposable
    {
        public void Dispose()
        {
            InternalEvent = null;
        }

        internal event Action InternalEvent;

        public IDisposable Subscribe(Action receive)
        {
            InternalEvent += receive;
            return new DisposeAction(() => InternalEvent -= receive);
        }

        public void Trigger()
        {
            var internalEvent = InternalEvent;
            internalEvent?.Invoke();
        }
    }
}