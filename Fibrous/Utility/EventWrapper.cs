namespace Fibrous.Utility
{
    using System;

    public sealed class EventWrapper<TEvent> : IPublishPort<TEvent>, IEvent<TEvent>
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