using System;

namespace Fibrous.Agents;

/// <summary>
///     Actor like abstraction.  Receives a single type of message through a channel
/// </summary>
/// <typeparam name="T"></typeparam>
public class ChannelAgent<T> : IDisposable
{
    protected IFiber Fiber;

    public ChannelAgent(IChannel<T> channel, Action<T> handler, Action<Exception> errorCallback = null)
    {
        Fiber = errorCallback == null ? new Fiber() : new Fiber(errorCallback);
        channel.Subscribe(Fiber, handler);
    }

    public ChannelAgent(IFiberFactory factory, IChannel<T> channel, Action<T> handler,
        Action<Exception> errorCallback = null)
    {
        Fiber = factory.CreateFiber(errorCallback);
        channel.Subscribe(Fiber, handler);
    }

    public void Dispose() => Fiber?.Dispose();
}
