using System;
using System.Threading.Tasks;

namespace Fibrous
{
    public static class EventExtensions
    {
        public static IDisposable SubscribeToEvent<T>(this IFiber fiber, object obj, string eventName,
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

            return new Unsubscriber(new DisposeAction(() => remove.Invoke(obj, addHandlerArgs)), fiber);
        }

        public static IDisposable SubscribeToEvent<T>(this IAsyncFiber fiber, object obj, string eventName,
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

            return new Unsubscriber(new DisposeAction(() => remove.Invoke(obj, addHandlerArgs)), fiber);
        }
    }
}