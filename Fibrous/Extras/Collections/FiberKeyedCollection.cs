using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fibrous.Extras.Collections;

public class FiberKeyedCollection<TKey, T> :
    ISnapshotSubscriberPort<ItemAction<T>, T[]>,
    IRequestPort<Func<T, bool>, T[]>,
    IDisposable
{
    private readonly SnapshotChannel<ItemAction<T>, T[]> _channel = new();
    private readonly Fiber _fiber;
    private readonly Dictionary<TKey, T> _items = new();
    private readonly Func<T, TKey> _keyGen;
    private readonly RequestChannel<Func<T, bool>, T[]> _request = new();

    public FiberKeyedCollection(Func<T, TKey> keyGen, IExecutor executor = null)
    {
        _keyGen = keyGen;
        _fiber = new Fiber(executor);
        _channel.ReplyToPrimingRequest(_fiber, ReplyAsync);
        _request.SetRequestHandler(_fiber, OnRequestAsync);
    }

    public void Dispose()
    {
        _fiber.Dispose();
        _channel.Dispose();
        _request.Dispose();
    }

    public IDisposable SendRequest(Func<T, bool> request, IFiber fiber, Func<T[], Task> onReply) =>
        _request.SendRequest(request, fiber, onReply);

    public IDisposable SendRequest(Func<T, bool> request, IFiber fiber, Action<T[]> onReply) =>
        SendRequest(request, fiber, items =>
        {
            onReply(items);
            return Task.CompletedTask;
        });

    public Task<T[]> SendRequestAsync(Func<T, bool> request) => _request.SendRequestAsync(request);

    public Task<Reply<T[]>> SendRequestAsync(Func<T, bool> request, CancellationToken cancellationToken) =>
        _request.SendRequestAsync(request, cancellationToken);

    public Task<Reply<T[]>> SendRequestAsync(Func<T, bool> request, TimeSpan timeout) =>
        _request.SendRequestAsync(request, timeout);

    public IDisposable Subscribe(
        IFiber fiber,
        Func<ItemAction<T>, Task> receive,
        Func<T[], Task> receiveSnapshot) =>
        _channel.Subscribe(fiber, receive, receiveSnapshot);

    public void Add(T item) => _fiber.Enqueue(() => AddItemAsync(item));

    public void Remove(T item) => _fiber.Enqueue(() => RemoveItemAsync(item));

    public Task<T[]> GetItemsAsync(Func<T, bool> request) => _request.SendRequestAsync(request);

    private Task OnRequestAsync(IRequest<Func<T, bool>, T[]> request)
    {
        request.Reply(_items.Values.Where(request.Request).ToArray());
        return Task.CompletedTask;
    }

    private Task RemoveItemAsync(T item)
    {
        bool removed = _items.Remove(_keyGen(item));
        if (removed)
        {
            _channel.Publish(new ItemAction<T>(ActionType.Remove, new[] { item }));
        }

        return Task.CompletedTask;
    }

    private Task AddItemAsync(T item)
    {
        TKey key = _keyGen(item);
        bool exists = _items.ContainsKey(key);
        _items[key] = item;
        _channel.Publish(new ItemAction<T>(exists ? ActionType.Update : ActionType.Add, new[] { item }));
        return Task.CompletedTask;
    }

    private Task<T[]> ReplyAsync() => Task.FromResult(_items.Values.ToArray());
}
