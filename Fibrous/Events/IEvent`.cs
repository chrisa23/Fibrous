using System;

namespace Fibrous;

/// <summary>
///     Simple subscribe event with Dispose() for unsubscribe.
/// </summary>
/// <typeparam name="TEvent"></typeparam>
public interface IEvent<TEvent> : IPublisherPort<TEvent>, IDisposable, IObservable<TEvent>
{
    IDisposable Subscribe(Action<TEvent> receive);
}

public sealed class Event<TEvent> : IEvent<TEvent>
{
    internal bool HasSubscriptions => InternalEvent != null;

    public IDisposable Subscribe(Action<TEvent> receive)
    {
        InternalEvent += receive;
        return new DisposeAction(() => InternalEvent -= receive);
    }

    public void Publish(TEvent msg)
    {
        Action<TEvent> internalEvent = InternalEvent;
        internalEvent?.Invoke(msg);
    }

    public void Dispose() => InternalEvent = null;

    public IDisposable Subscribe(IObserver<TEvent> observer)
    {
        InternalEvent += observer.OnNext;
        return new DisposeAction(() => InternalEvent -= observer.OnNext);
    }

    private event Action<TEvent> InternalEvent;
}
