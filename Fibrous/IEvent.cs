using System;

namespace Fibrous
{
    /// <summary>
    ///     Simple subscribe event with Dispose() for unsubscribe.
    /// </summary>
    public interface IEvent : IEventTrigger, IDisposable
    {
        IDisposable Subscribe(Action receive);
    }

    public sealed class Event : IEvent
    {
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

        public void Dispose()
        {
            InternalEvent = null;
        }

        internal event Action InternalEvent;
        public bool HasSubscriptions => InternalEvent != null;
    }
}