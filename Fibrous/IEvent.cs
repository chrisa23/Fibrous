using System;

namespace Fibrous;

public interface IEventTrigger
{
    void Trigger();
}

/// <summary>
///     Simple subscribe event with Dispose() for unsubscribe.
/// </summary>
public interface IEvent : IEventTrigger, IDisposable
{
    IDisposable Subscribe(Action receive);
}

/// <summary>
///     Simple subscribe event with Dispose() for unsubscribe.
/// </summary>
public interface IEvent<TEvent> : IPublisherPort<TEvent>, IDisposable, IObservable<TEvent>
{
    IDisposable Subscribe(Action<TEvent> receive);
}
