using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fibrous.Extras.Collections;

public class FiberDictionary<TKey, T> :
    ISnapshotSubscriberPort<ItemAction<KeyValuePair<TKey, T>>, KeyValuePair<TKey, T>[]>,
    IRequestPort<Func<TKey, bool>, KeyValuePair<TKey, T>[]>,
    IDisposable
{
    private readonly Channel<KeyValuePair<TKey, T>> _add = new();
    private readonly SnapshotChannel<ItemAction<KeyValuePair<TKey, T>>, KeyValuePair<TKey, T>[]> _channel = new();
    private readonly Fiber _fiber;
    private readonly Dictionary<TKey, T> _items = new();
    private readonly Channel<TKey> _remove = new();
    private readonly RequestChannel<Func<TKey, bool>, KeyValuePair<TKey, T>[]> _request = new();

    public FiberDictionary(IExecutor executor = null)
    {
        _fiber = new Fiber(executor);
        _channel.ReplyToPrimingRequest(_fiber, ReplyAsync);
        _add.Subscribe(_fiber, AddItemAsync);
        _remove.Subscribe(_fiber, RemoveItemAsync);
        _request.SetRequestHandler(_fiber, OnRequestAsync);
    }

    public void Dispose()
    {
        _fiber.Dispose();
        _add.Dispose();
        _remove.Dispose();
        _channel.Dispose();
        _request.Dispose();
    }

    public IDisposable SendRequest(
        Func<TKey, bool> request,
        IFiber fiber,
        Func<KeyValuePair<TKey, T>[], Task> onReply) =>
        _request.SendRequest(request, fiber, onReply);

    public IDisposable SendRequest(
        Func<TKey, bool> request,
        IFiber fiber,
        Action<KeyValuePair<TKey, T>[]> onReply) =>
        SendRequest(request, fiber, items =>
        {
            onReply(items);
            return Task.CompletedTask;
        });

    public Task<KeyValuePair<TKey, T>[]> SendRequestAsync(Func<TKey, bool> request) =>
        _request.SendRequestAsync(request);

    public Task<Reply<KeyValuePair<TKey, T>[]>> SendRequestAsync(
        Func<TKey, bool> request,
        CancellationToken cancellationToken) =>
        _request.SendRequestAsync(request, cancellationToken);

    public Task<Reply<KeyValuePair<TKey, T>[]>> SendRequestAsync(Func<TKey, bool> request, TimeSpan timeout) =>
        _request.SendRequestAsync(request, timeout);

    public IDisposable Subscribe(
        IFiber fiber,
        Func<ItemAction<KeyValuePair<TKey, T>>, Task> receive,
        Func<KeyValuePair<TKey, T>[], Task> receiveSnapshot) =>
        _channel.Subscribe(fiber, receive, receiveSnapshot);

    public void Add(KeyValuePair<TKey, T> item) => _add.Publish(item);

    public void Add(TKey key, T item) => _add.Publish(new KeyValuePair<TKey, T>(key, item));

    public void Remove(TKey item) => _remove.Publish(item);

    public void Clear() =>
        _fiber.Enqueue(() =>
        {
            _items.Clear();
            _channel.Publish(
                new ItemAction<KeyValuePair<TKey, T>>(ActionType.Clear, Array.Empty<KeyValuePair<TKey, T>>()));
        });

    public void AddRange(IEnumerable<KeyValuePair<TKey, T>> items) =>
        _fiber.Enqueue(() =>
        {
            List<KeyValuePair<TKey, T>> added = new();
            List<KeyValuePair<TKey, T>> updated = new();
            foreach (KeyValuePair<TKey, T> item in items)
            {
                if (_items.ContainsKey(item.Key))
                {
                    updated.Add(item);
                }
                else
                {
                    added.Add(item);
                }

                _items[item.Key] = item.Value;
            }

            if (added.Count > 0)
            {
                _channel.Publish(new ItemAction<KeyValuePair<TKey, T>>(ActionType.Add, added.ToArray()));
            }

            if (updated.Count > 0)
            {
                _channel.Publish(new ItemAction<KeyValuePair<TKey, T>>(ActionType.Update, updated.ToArray()));
            }
        });

    public Task<KeyValuePair<TKey, T>[]> GetItemsAsync(Func<TKey, bool> request) => _request.SendRequestAsync(request);

    public IDisposable SubscribeLocalCopy(IFiber fiber, Dictionary<TKey, T> localDict, Action updateCallback) =>
        Subscribe(fiber, CreateReceive(localDict, updateCallback), CreateSnapshot(localDict, updateCallback));

    private Task OnRequestAsync(IRequest<Func<TKey, bool>, KeyValuePair<TKey, T>[]> request)
    {
        request.Reply(_items.Where(x => request.Request(x.Key)).ToArray());
        return Task.CompletedTask;
    }

    private Task RemoveItemAsync(TKey key)
    {
        _items.TryGetValue(key, out T data);
        bool removed = _items.Remove(key);
        if (removed)
        {
            _channel.Publish(new ItemAction<KeyValuePair<TKey, T>>(
                ActionType.Remove,
                new[] { new KeyValuePair<TKey, T>(key, data) }));
        }

        return Task.CompletedTask;
    }

    private Task AddItemAsync(KeyValuePair<TKey, T> item)
    {
        bool exists = _items.ContainsKey(item.Key);
        _items[item.Key] = item.Value;
        _channel.Publish(new ItemAction<KeyValuePair<TKey, T>>(
            exists ? ActionType.Update : ActionType.Add,
            new[] { item }));
        return Task.CompletedTask;
    }

    private Task<KeyValuePair<TKey, T>[]> ReplyAsync()
    {
        try
        {
            return Task.FromResult(_items.ToArray());
        }
        catch (Exception exception)
        {
            return Task.FromException<KeyValuePair<TKey, T>[]>(exception);
        }
    }

    private static Func<ItemAction<KeyValuePair<TKey, T>>, Task> CreateReceive(
        Dictionary<TKey, T> localDict,
        Action updateCallback) =>
        action =>
        {
            UpdateLocal(localDict, action);
            updateCallback();
            return Task.CompletedTask;
        };

    private static void UpdateLocal(Dictionary<TKey, T> localDict, ItemAction<KeyValuePair<TKey, T>> action)
    {
        switch (action.ActionType)
        {
            case ActionType.Add:
            case ActionType.Update:
                foreach (KeyValuePair<TKey, T> item in action.Items)
                {
                    localDict[item.Key] = item.Value;
                }

                break;
            case ActionType.Remove:
                foreach (KeyValuePair<TKey, T> item in action.Items)
                {
                    localDict.Remove(item.Key);
                }

                break;
            case ActionType.Clear:
                localDict.Clear();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static Func<KeyValuePair<TKey, T>[], Task> CreateSnapshot(
        Dictionary<TKey, T> localDict,
        Action updateCallback) =>
        items =>
        {
            foreach (KeyValuePair<TKey, T> item in items)
            {
                localDict[item.Key] = item.Value;
            }

            updateCallback();
            return Task.CompletedTask;
        };
}
