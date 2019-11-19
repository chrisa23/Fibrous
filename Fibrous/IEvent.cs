using System;
using System.Threading.Tasks;

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
            var disposeAction = new DisposeAction(() => InternalEvent -= receive);
            return disposeAction;
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
            var disposeAction = new DisposeAction(() => InternalEvent -= receive);
            return disposeAction;
        }

        public void Trigger()
        {
            var internalEvent = InternalEvent;
            internalEvent?.Invoke();
        }
    }

    public static class EventExtensions
    {
        internal static Action<T> Receive<T>(this IExecutionContext fiber, Action<T> receive)
        {
            //how to avoid this closure...
            return msg => fiber.Enqueue(() => receive(msg));
        }

        internal static Action<T> Receive<T>(this IAsyncExecutionContext fiber, Func<T, Task> receive)
        {
            //how to avoid this closure...
            return msg => fiber.Enqueue(() => receive(msg));
        }

        public static IDisposable SubscribeToEvent<T>(this IExecutionContext fiber, object obj, string eventName,
            Action<T> receive)
        {
            var evt = obj.GetType().GetEvent(eventName);
            var add = evt.GetAddMethod();
            var remove = evt.GetRemoveMethod();

            void Action(T msg)
            {
                fiber.Enqueue(() => receive(msg));
            }

            object[] addHandlerArgs = {(Action<T>) Action};
            add.Invoke(obj, addHandlerArgs);

            return new DisposeAction(() => remove.Invoke(obj, addHandlerArgs));
        }

        public static IDisposable SubscribeToEvent<T>(this IAsyncExecutionContext fiber, object obj, string eventName,
            Func<T, Task> receive)
        {
            var evt = obj.GetType().GetEvent(eventName);
            var add = evt.GetAddMethod();
            var remove = evt.GetRemoveMethod();

            void Action(T msg)
            {
                fiber.Enqueue(() => receive(msg));
            }

            object[] addHandlerArgs = {(Action<T>) Action};
            add.Invoke(obj, addHandlerArgs);

            return new DisposeAction(() => remove.Invoke(obj, addHandlerArgs));
        }
    }
}