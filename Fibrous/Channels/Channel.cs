namespace Fibrous.Channels
{
    using System;

    /// <summary>
    /// Channels are in memory conduits...
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class Channel<T> : IChannel<T>
    {
        private readonly EventChannel<T> _internalChannel = new EventChannel<T>();

        public bool Publish(T msg)
        {
            return _internalChannel.Publish(msg);
        }

        public IDisposable Subscribe(IFiber fiber, Action<T> receive)
        {
            IDisposable disposable = _internalChannel.Subscribe(msg => fiber.Enqueue(() => receive(msg)));
            return new Unsubscriber<T>(disposable, fiber);
        }

        private sealed class EventChannel<TEvent> : IPublisherPort<TEvent>
        {
            private event Action<TEvent> InternalEvent;

            public IDisposable Subscribe(Action<TEvent> receive)
            {
                InternalEvent += receive;
                return new DisposeAction(() => InternalEvent -= receive);
            }

            public bool Publish(TEvent msg)
            {
                Action<TEvent> internalEvent = InternalEvent;
                if (internalEvent != null)
                {
                    internalEvent(msg);
                    return true;
                }
                return false;
            }

            private sealed class DisposeAction : IDisposable
            {
                private readonly Action _action;

                public DisposeAction(Action action)
                {
                    _action = action;
                }

                public void Dispose()
                {
                    _action();
                }
            }
        }
    }
}