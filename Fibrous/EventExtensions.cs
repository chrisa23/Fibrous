using System;
using System.Threading.Tasks;

namespace Fibrous
{
    public static class EventExtensions
    {
        /// <summary>
        /// Subscribe a fiber to an Action based event.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fiber"></param>
        /// <param name="obj"></param>
        /// <param name="eventName"></param>
        /// <param name="receive"></param>
        /// <returns></returns>
        public static IDisposable SubscribeToEvent<T>(this IFiber fiber, object obj, string eventName,
            Action<T> receive)
        {
            var evt = obj.GetType().GetEvent(eventName);
            var add = evt.GetAddMethod();
            var remove = evt.GetRemoveMethod();

            void Action(T msg) => fiber.Enqueue(() => receive(msg));

            object[] addHandlerArgs = {(Action<T>) Action};
            add.Invoke(obj, addHandlerArgs);

            return new Unsubscriber(new DisposeAction(() => remove.Invoke(obj, addHandlerArgs)), fiber);
        }

        /// <summary>
        /// Subscribe an AsyncFiber to an Action based event
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fiber"></param>
        /// <param name="obj"></param>
        /// <param name="eventName"></param>
        /// <param name="receive"></param>
        /// <returns></returns>
        public static IDisposable SubscribeToEvent<T>(this IAsyncFiber fiber, object obj, string eventName,
            Func<T, Task> receive)
        {
            var evt = obj.GetType().GetEvent(eventName);
            var add = evt.GetAddMethod();
            var remove = evt.GetRemoveMethod();

            void Action(T msg) => fiber.Enqueue(() => receive(msg));

            object[] addHandlerArgs = {(Action<T>) Action};
            add.Invoke(obj, addHandlerArgs);

            return new Unsubscriber(new DisposeAction(() => remove.Invoke(obj, addHandlerArgs)), fiber);
        }

        /// <summary>
        /// Subscribe a fiber to an Action based event.
        /// </summary>
        /// <param name="fiber"></param>
        /// <param name="obj"></param>
        /// <param name="eventName"></param>
        /// <param name="receive"></param>
        /// <returns></returns>
        public static IDisposable SubscribeToEvent(this IFiber fiber, object obj, string eventName,
            Action receive)
        {
            var evt = obj.GetType().GetEvent(eventName);
            var add = evt.GetAddMethod();
            var remove = evt.GetRemoveMethod();

            void Action() => fiber.Enqueue(receive);

            object[] addHandlerArgs = { (Action)Action };
            add.Invoke(obj, addHandlerArgs);

            return new Unsubscriber(new DisposeAction(() => remove.Invoke(obj, addHandlerArgs)), fiber);
        }

        /// <summary>
        /// Subscribe an AsyncFiber to an Action based event
        /// </summary>
        /// <param name="fiber"></param>
        /// <param name="obj"></param>
        /// <param name="eventName"></param>
        /// <param name="receive"></param>
        /// <returns></returns>
        public static IDisposable SubscribeToEvent(this IAsyncFiber fiber, object obj, string eventName,
            Func<Task> receive)
        {
            var evt = obj.GetType().GetEvent(eventName);
            var add = evt.GetAddMethod();
            var remove = evt.GetRemoveMethod();

            void Action() => fiber.Enqueue(receive);

            object[] addHandlerArgs = { (Action)Action };
            add.Invoke(obj, addHandlerArgs);

            return new Unsubscriber(new DisposeAction(() => remove.Invoke(obj, addHandlerArgs)), fiber);
        }
    }
}