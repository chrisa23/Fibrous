namespace Fibrous
{
    using System;

    public sealed class Event<TEvent> : IEvent<TEvent>
    {
        private event Action<TEvent> InternalEvent;

        public IDisposable Subscribe(Action<TEvent> receive)
        {
            InternalEvent += receive;
            var disposeAction = new DisposeAction(() => InternalEvent -= receive);
            return disposeAction;
        }

        public void Publish(TEvent msg)
        {
            Action<TEvent> internalEvent = InternalEvent;
            internalEvent?.Invoke(msg);
        }

        public void Dispose()
        {
            InternalEvent = null;
        }

        internal bool HasSubscriptions()
        {
            return InternalEvent != null;
        }
    }

    public sealed class Event : IDisposable
    {
        internal event Action InternalEvent;

        public IDisposable Subscribe(Action receive)
        {
            InternalEvent += receive;
            var disposeAction = new DisposeAction(() => InternalEvent -= receive);
            return disposeAction;
        }

        public void Trigger()
        {
            Action internalEvent = InternalEvent;
            internalEvent?.Invoke();
        }

        public void Dispose()
        {
            InternalEvent = null;
        }
    }
}