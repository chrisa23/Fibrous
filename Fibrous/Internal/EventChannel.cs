using System;

namespace Fibrous.Internal
{
    /// <summary>
    /// Simple conduit for events with IDisposable subscribing
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    internal sealed class EventChannel<TEvent> : IPublisherPort<TEvent>
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

        public int NumSubscribers
        {
            get { return InternalEvent == null ? 0 : InternalEvent.GetInvocationList().Length; }
        }
    }
}