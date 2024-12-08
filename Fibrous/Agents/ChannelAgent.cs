using System;
using System.Threading.Tasks;

namespace Fibrous.Agents;

/// <summary>
///     Actor like abstraction.  Receives a single type of message through a channel
/// </summary>
/// <typeparam name="T"></typeparam>
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

    public void Dispose() => Fiber?.Dispose();
}
