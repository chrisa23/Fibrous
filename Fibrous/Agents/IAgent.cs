using System;

namespace Fibrous.Agents;

/// <summary>
///     Actor-like abstraction that receives a single type of published message.
/// </summary>
public interface IAgent<in T> : IPublisherPort<T>, IDisposable
{
}
