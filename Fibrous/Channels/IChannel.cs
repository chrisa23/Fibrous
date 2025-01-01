using System;

namespace Fibrous;

/// <summary>
///     IChannels are in-memory conduits for messages
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IChannel<T> : IPublisherPort<T>, ISubscriberPort<T>, IDisposable
{
}
