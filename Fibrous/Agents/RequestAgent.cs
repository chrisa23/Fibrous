using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fibrous.Agents;

/// <summary>
///     Request/reply agent that owns a fiber and delegates requests to a supplied handler.
/// </summary>
public class RequestAgent<TRequest, TReply> : IRequestAgent<TRequest, TReply>
{
    private readonly IRequestPort<TRequest, TReply> _channel;

    protected IFiber Fiber;

    public RequestAgent(Func<IRequest<TRequest, TReply>, Task> handler, Action<Exception> callback)
    {
        Fiber = new Fiber(callback);
        _channel = Fiber.NewRequestPort(handler);
    }

    public RequestAgent(
        IFiberFactory factory,
        Func<IRequest<TRequest, TReply>, Task> handler,
        Action<Exception> callback)
    {
        Fiber = factory.CreateFiber(callback);
        _channel = Fiber.NewRequestPort(handler);
    }

    public IDisposable SendRequest(TRequest request, IFiber fiber, Func<TReply, Task> onReply) =>
        _channel.SendRequest(request, fiber, onReply);

    public IDisposable SendRequest(TRequest request, IFiber fiber, Action<TReply> onReply) =>
        _channel.SendRequest(request, fiber, onReply);

    public Task<TReply> SendRequestAsync(TRequest request) => _channel.SendRequestAsync(request);

    public Task<Reply<TReply>> SendRequestAsync(TRequest request, CancellationToken cancellationToken) =>
        _channel.SendRequestAsync(request, cancellationToken);

    public Task<Reply<TReply>> SendRequestAsync(TRequest request, TimeSpan timeout) =>
        _channel.SendRequestAsync(request, timeout);

    public void Dispose() => Fiber.Dispose();
}
