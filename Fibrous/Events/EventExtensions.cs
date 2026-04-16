using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Fibrous;

public static class EventExtensions
{
    /// <summary>
    ///     Subscribes a fiber to an <see cref="Action{T}" />-based event by name.
    /// </summary>
    public static IDisposable SubscribeToEvent<T>(
        this IFiber fiber,
        object obj,
        string eventName,
        Func<T, Task> receive)
    {
        (MethodInfo add, MethodInfo remove) = GetEventAccessors(obj, eventName);

        void Handler(T message) => fiber.Enqueue(() => receive(message));

        object[] handlerArgs = { (Action<T>)Handler };
        add.Invoke(obj, handlerArgs);

        return new Unsubscriber(new DisposeAction(() => remove.Invoke(obj, handlerArgs)), fiber);
    }

    /// <summary>
    ///     Subscribes a fiber to an <see cref="Action" />-based event by name.
    /// </summary>
    public static IDisposable SubscribeToEvent(
        this IFiber fiber,
        object obj,
        string eventName,
        Func<Task> receive)
    {
        (MethodInfo add, MethodInfo remove) = GetEventAccessors(obj, eventName);

        void Handler()
        {
            fiber.Enqueue(receive);
        }

        object[] handlerArgs = { (Action)Handler };
        add.Invoke(obj, handlerArgs);

        return new Unsubscriber(new DisposeAction(() => remove.Invoke(obj, handlerArgs)), fiber);
    }

    /// <summary>
    ///     Subscribes a fiber to an <see cref="Action{T}" />-based event by name.
    /// </summary>
    public static IDisposable SubscribeToEvent<T>(
        this IFiber fiber,
        object obj,
        string eventName,
        Action<T> receive) =>
        SubscribeToEvent(fiber, obj, eventName, receive.ToAsync());

    /// <summary>
    ///     Subscribes a fiber to an <see cref="Action" />-based event by name.
    /// </summary>
    public static IDisposable SubscribeToEvent(
        this IFiber fiber,
        object obj,
        string eventName,
        Action receive) =>
        SubscribeToEvent(fiber, obj, eventName, receive.ToAsync());

    private static (MethodInfo add, MethodInfo remove) GetEventAccessors(object target, string eventName)
    {
        EventInfo evt = target.GetType().GetEvent(eventName);
        MethodInfo add = evt.GetAddMethod();
        MethodInfo remove = evt.GetRemoveMethod();
        return (add, remove);
    }
}
