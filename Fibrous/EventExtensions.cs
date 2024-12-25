using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Fibrous;

public static class EventExtensions
{
    /// <summary>
    ///     Subscribe an AsyncFiber to an Action based event
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="fiber"></param>
    /// <param name="obj"></param>
    /// <param name="eventName"></param>
    /// <param name="receive"></param>
    /// <returns></returns>
    public static IDisposable SubscribeToEvent<T>(this IFiber fiber, object obj, string eventName,
        Func<T, Task> receive)
    {
        EventInfo evt = obj.GetType().GetEvent(eventName);
        MethodInfo add = evt.GetAddMethod();
        MethodInfo remove = evt.GetRemoveMethod();

        void Action(T msg) => fiber.Enqueue(() => receive(msg));

        add.Invoke(obj, [(Action<T>)Action]);

        return new Unsubscriber(new DisposeAction(() => remove.Invoke(obj, [(Action<T>)Action])), fiber);
    }


    /// <summary>
    ///     Subscribe an AsyncFiber to an Action based event
    /// </summary>
    /// <param name="fiber"></param>
    /// <param name="obj"></param>
    /// <param name="eventName"></param>
    /// <param name="receive"></param>
    /// <returns></returns>
    public static IDisposable SubscribeToEvent(this IFiber fiber, object obj, string eventName,
        Func<Task> receive)
    {
        EventInfo evt = obj.GetType().GetEvent(eventName);
        MethodInfo add = evt.GetAddMethod();
        MethodInfo remove = evt.GetRemoveMethod();

        void Action()
        {
            fiber.Enqueue(receive);
        }

        object[] addHandlerArgs = {(Action)Action};
        add.Invoke(obj, addHandlerArgs);

        return new Unsubscriber(new DisposeAction(() => remove.Invoke(obj, addHandlerArgs)), fiber);
    }


    /// <summary>
    ///     Subscribe an AsyncFiber to an Action based event
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="fiber"></param>
    /// <param name="obj"></param>
    /// <param name="eventName"></param>
    /// <param name="receive"></param>
    /// <returns></returns>
    public static IDisposable SubscribeToEvent<T>(this IFiber fiber, object obj, string eventName,
        Action<T> receive) =>
        SubscribeToEvent(fiber, obj, eventName, receive.ToAsync());



    /// <summary>
    ///     Subscribe an AsyncFiber to an Action based event
    /// </summary>
    /// <param name="fiber"></param>
    /// <param name="obj"></param>
    /// <param name="eventName"></param>
    /// <param name="receive"></param>
    /// <returns></returns>
    public static IDisposable SubscribeToEvent(this IFiber fiber, object obj, string eventName,
        Action receive) =>
        SubscribeToEvent(fiber, obj, eventName, receive.ToAsync());
}
