using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Fibrous;

/// <summary>
///     Injectable event hub where classes implementing a combination of
///     IHandle or IHandleAsync can auto wire up to receive events publish on the hub.
/// </summary>
public interface IEventHub
{
    IDisposable Subscribe(IFiber fiber, object handler);
    void Publish<T>(T msg);
}

/// <summary>
///     For use with IEventHub to auto wire events
///     Denotes a class which can handle a particular type of message.
/// </summary>
/// <typeparam name="TMessage">The type of message to handle.</typeparam>
// ReSharper disable once TypeParameterCanBeVariant
public interface IHandle<TMessage>
{
    void Handle(TMessage message);
}

/// <summary>
///     For use with IEventHub to auto wire events
///     Denotes a class which can handle a particular type of message.
/// </summary>
/// <typeparam name="TMessage">The type of message to handle.</typeparam>
// ReSharper disable once TypeParameterCanBeVariant
public interface IHandleAsync<TMessage>
{
    Task HandleAsync(TMessage message);
}

/// <summary>
///     Implementation of IEventHub
///     Performance is slower on publish due to GetType and channel lookup
/// </summary>
public sealed class EventHub : IEventHub
{
    private readonly ConcurrentDictionary<Type, object> _channels = new();

    public IDisposable Subscribe(IFiber fiber, object handler)
    {
        IDisposable disposable = SetupHandlers(handler, fiber, false);

        return new Unsubscriber(disposable, fiber);
    }

    //20 ns for this with no subscribers (now 16ns with changes)
    public void Publish<T>(T msg)
    {
        Type type = typeof(T);

        if (!_channels.ContainsKey(type))
        {
            return;
        }

        IChannel<T> channel = (IChannel<T>)_channels[type];
        channel.Publish(msg);
    }

    private IDisposable SetupHandlers(object handler, object fiber, bool regular)
    {
        Type interfaceType = regular ? typeof(IHandle<>) : typeof(IHandleAsync<>);
        string subMethod = regular ? "SubscribeToChannel" : "AsyncSubscribeToChannel";
        IEnumerable<Type> interfaces = handler.GetType().GetTypeInfo()
            .ImplementedInterfaces.Where(x =>
                x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == interfaceType);

        Disposables disposables = new();

        foreach (Type @interface in interfaces)
        {
            Type type = @interface.GetTypeInfo().GenericTypeArguments[0];
            MethodInfo method = @interface.GetRuntimeMethod(regular ? "Handle" : "HandleAsync", new[] {type});

            if (method == null)
            {
                continue;
            }

            MethodInfo sub = GetType().GetTypeInfo().GetDeclaredMethod(subMethod).MakeGenericMethod(type);

            IDisposable dispose = sub.Invoke(this, new[] {fiber, handler}) as IDisposable;
            disposables.Add(dispose);
        }

        return disposables;
    }


    // ReSharper disable once UnusedMember.Local
    private IDisposable AsyncSubscribeToChannel<T>(IFiber fiber, IHandleAsync<T> receive)
    {
        Type type = typeof(T);
        IChannel<T> channel = (IChannel<T>)_channels.GetOrAdd(type, _ => new Channel<T>());
        return channel.Subscribe(fiber, receive.HandleAsync);
    }

    internal bool HasSubscriptions<T>()
    {
        Type type = typeof(T);

        if (!_channels.ContainsKey(type))
        {
            return false;
        }

        Channel<T> channel = (Channel<T>)_channels[type];
        return channel.HasSubscriptions;
    }
}
