using System;
using System.Threading.Tasks;

namespace Fibrous.Agents;

/// <summary>
///     Agent that owns a fiber and forwards published messages to a supplied handler.
/// </summary>
public class Agent<T> : IAgent<T>
{
    private readonly Func<T, Task> _handler;

    protected IFiber Fiber;

    public Agent(Func<T, Task> handler, Action<Exception> callback)
    {
        _handler = handler;
        Fiber = new Fiber(callback);
    }

    public Agent(IFiberFactory factory, Func<T, Task> handler, Action<Exception> callback)
    {
        _handler = handler;
        Fiber = factory.CreateFiber(callback);
    }

    public void Publish(T message) => Fiber.Enqueue(() => _handler(message));

    public void Dispose() => Fiber.Dispose();
}
