namespace Fibrous
{
    using System;

    /// <summary>
    /// Simple subscribable event with Dispose() for unsubscription.  
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    public interface IEvent<TEvent> : IPublisherPort<TEvent>
    {
        IDisposable Subscribe(Action<TEvent> receive);
    }

    public sealed class Event<TEvent> : IEvent<TEvent>, IDisposable
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

        public void Dispose()
        {
            InternalEvent = null;
        }
    }

    public sealed class Event : IDisposable
    {
        private event Action InternalEvent;

        public IDisposable Subscribe(Action receive)
        {
            InternalEvent += receive;
            var disposeAction = new DisposeAction(() => InternalEvent -= receive);
            return disposeAction;
        }

        public bool Trigger()
        {
            Action internalEvent = InternalEvent;
            if (internalEvent != null)
            {
                internalEvent();
                return true;
            }
            return false;
        }

        public void Dispose()
        {
            InternalEvent = null;
        }
    }
}