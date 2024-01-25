﻿using System;
using System.Threading.Tasks;

namespace Fibrous.Agents;

/// <summary>
///     Agent using injected handler function.
/// </summary>
/// <typeparam name="T"></typeparam>
public class Agent<T> : IAgent<T>
{
    private readonly Func<T, Task> _handler;
    protected IAsyncFiber Fiber;

    public Agent(Func<T, Task> handler, Action<Exception> callback)
    {
        _handler = handler;
        Fiber = new AsyncFiber(callback);
    }

    public Agent(IFiberFactory factory, Func<T, Task> handler, Action<Exception> callback)
    {
        _handler = handler;
        Fiber = factory.CreateAsyncFiber(callback);
    }

    public void Publish(T msg) => Fiber.Enqueue(() => _handler(msg));

    public void Dispose() => Fiber?.Dispose();
}
