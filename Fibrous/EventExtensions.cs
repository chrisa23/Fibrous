namespace Fibrous
{
    using System;
    using System.Reflection;

    public static class EventExtensions
    {
        internal static Action<T> Receive<T>(this IExecutionContext fiber, Action<T> receive)
        {
            return msg => fiber.Enqueue(() => receive(msg));
        }

        public static IDisposable SubscribeToEvent<T>(this IExecutionContext fiber, object obj, string eventName, Action<T> receive)
        {
            EventInfo evt = obj.GetType().GetEvent(eventName);
            MethodInfo add = evt.GetAddMethod();
            MethodInfo remove = evt.GetRemoveMethod();
            void Action(T msg) => fiber.Enqueue(() => receive(msg));
            object[] addHandlerArgs = { (Action<T>)Action };
            add.Invoke(obj, addHandlerArgs);
            return new DisposeAction(() => remove.Invoke(obj, addHandlerArgs));
        }
    }
}