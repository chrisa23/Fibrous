using System;
using System.Threading.Tasks;

namespace Fibrous.Agents;

/// <summary>
///     Actor-like abstraction that subscribes a dedicated fiber to a channel.
/// </summary>
public class ChannelAgent<T> : IDisposable
{
    protected IFiber Fiber;

    public ChannelAgent(IChannel<T> channel, Func<T, Task> handler, Action<Exception> errorCallback)
    {
        Fiber = new Fiber(errorCallback);
        channel.Subscribe(Fiber, handler);
    }

    public ChannelAgent(IFiberFactory factory, IChannel<T> channel, Func<T, Task> handler,
        Action<Exception> errorCallback)
    {
        Fiber = factory.CreateFiber(errorCallback);
        channel.Subscribe(Fiber, handler);
    }

    public void Dispose() => Fiber.Dispose();
}
