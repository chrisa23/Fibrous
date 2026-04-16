using System;

namespace Fibrous.Agents;

/// <summary>
///     Actor-like abstraction for request/reply messaging.
/// </summary>
public interface IRequestAgent<in TRequest, TReply> : IRequestPort<TRequest, TReply>, IDisposable
{
}
