using System;
using System.Threading.Tasks;

namespace Fibrous.Agents;

/// <summary>
///     Agent using injected handler function.
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TReply"></typeparam>
public class RequestAgent<TRequest, TReply> : IRequestAgent<TRequest, TReply>
{
    private readonly IRequestPort<TRequest, TReply> _channel;
    protected IFiber Fiber;

    public RequestAgent(Action<IRequest<TRequest, TReply>> handler, Action<Exception> errorHandler = null)
    {
        Fiber = errorHandler == null ? new Fiber() : new Fiber(errorHandler);
        _channel = Fiber.NewRequestPort(handler);
    }

    public RequestAgent(IFiberFactory factory, Action<IRequest<TRequest, TReply>> handler,
        Action<Exception> errorHandler = null)
    {
        Fiber = factory.CreateFiber(errorHandler);
        _channel = Fiber.NewRequestPort(handler);
    }

    public IDisposable SendRequest(TRequest request, IFiber fiber, Action<TReply> onReply) =>
        _channel.SendRequest(request, fiber, onReply);

    public IDisposable SendRequest(TRequest request, IAsyncFiber fiber, Func<TReply, Task> onReply) =>
        _channel.SendRequest(request, fiber, onReply);

    public Task<TReply> SendRequestAsync(TRequest request) => _channel.SendRequestAsync(request);

    public Task<Reply<TReply>> SendRequestAsync(TRequest request, TimeSpan timeout) =>
        _channel.SendRequestAsync(request, timeout);

    public void Dispose() => Fiber?.Dispose();
}
