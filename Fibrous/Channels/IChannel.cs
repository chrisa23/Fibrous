using System;

namespace Fibrous;

/// <summary>
///     In-memory conduit for publishing and subscribing to messages.
/// </summary>
public interface IChannel<T> : IPublisherPort<T>, ISubscriberPort<T>, IDisposable
{
}
