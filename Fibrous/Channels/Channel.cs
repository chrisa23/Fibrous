namespace Fibrous.Channels
{
    using System;
    using Fibrous.Utility;

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
            IDisposable disposable = _internalChannel.Subscribe(Receive(fiber, receive));
            return new Unsubscriber(disposable, fiber);
        }

        private static Action<T> Receive(IFiber fiber, Action<T> receive)
        {
            return msg => fiber.Enqueue(() => receive(msg));
        }

        private sealed class EventChannel<TEvent>
        {
            private event Action<TEvent> InternalEvent;

            public IDisposable Subscribe(Action<TEvent> receive)
            {
                InternalEvent += receive;
                var disposeAction = new DisposeAction(() => InternalEvent -= receive);
                return disposeAction;
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
        }
    }
}