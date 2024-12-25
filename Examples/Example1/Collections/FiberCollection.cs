using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fibrous;

namespace Example1.Collections;

/// <summary>
///     Collection class that can be monitored and provides a snapshot on subscription.  Can also be queried with a
///     predicate
/// </summary>
/// <typeparam name="T"></typeparam>
public class FiberCollection<T> : ISnapshotSubscriberPort<ItemAction<T>, T[]>, IRequestPort<Func<T, bool>, T[]>,
    IDisposable
{
    private readonly ISnapshotChannel<ItemAction<T>, T[]> _channel = new SnapshotChannel<ItemAction<T>, T[]>();
    private readonly IFiber _fiber;
    private readonly List<T> _items = new();
    private readonly IRequestChannel<Func<T, bool>, T[]> _request = new RequestChannel<Func<T, bool>, T[]>();

    public FiberCollection(IExecutor executor = null)
    {
        _fiber = new Fiber(executor);
        _channel.ReplyToPrimingRequest(_fiber, Reply);
        _request.SetRequestHandler(_fiber, OnRequest);
    }

    public void Dispose() => _fiber.Dispose();

    public IDisposable SendRequest(Func<T, bool> request, IFiber fiber, Func<T[], Task> onReply) =>
        _request.SendRequest(request, fiber, onReply);

    public IDisposable SendRequest(Func<T, bool> request, IFiber fiber, Action<T[]> onReply) =>
        SendRequest(request, fiber,onReply.ToAsync());

    public Task<T[]> SendRequestAsync(Func<T, bool> request) => _request.SendRequestAsync(request);

    public Task<Reply<T[]>> SendRequestAsync(Func<T, bool> request, TimeSpan timeout) =>
        _request.SendRequestAsync(request, timeout);



    public IDisposable Subscribe(IFiber fiber, Func<ItemAction<T>, Task> receive,
        Func<T[], Task> receiveSnapshot) => _channel.Subscribe(fiber, receive, receiveSnapshot);

    public void Clear() =>
        _fiber.Enqueue(() =>
        {
            _items.Clear();
            _channel.Publish(new ItemAction<T>(ActionType.Clear, new T[] { }));
        });

    public void AddRange(IEnumerable<T> items) =>
        _fiber.Enqueue(() =>
        {
            T[] itemArray = items.ToArray();
            _items.AddRange(itemArray);
            _channel.Publish(new ItemAction<T>(ActionType.Add, itemArray));
        });

    public void Add(T item) =>
        _fiber.Enqueue(() =>
        {
            _items.Add(item);
            _channel.Publish(new ItemAction<T>(ActionType.Add, new[] {item}));
        });

    public void Remove(T item) =>
        _fiber.Enqueue(() =>
        {
            bool removed = _items.Remove(item);
            if (removed)
            {
                _channel.Publish(new ItemAction<T>(ActionType.Remove, new[] {item}));
            }
        });

    public Task<T[]> GetItemsAsync(Func<T, bool> request) => _request.SendRequestAsync(request);

    private async Task  OnRequest(IRequest<Func<T, bool>, T[]> request) =>
        request.Reply(_items.Where(request.Request).ToArray());

    private async Task<T[]> Reply() => _items.ToArray();

    public IDisposable SubscribeLocalCopy(IFiber fiber, List<T> local, Action updateCallback) =>
        Subscribe(fiber, CreateReceive(local, updateCallback), CreateSnapshot(local, updateCallback));

    private static Func<T[], Task> CreateSnapshot(List<T> local,
        Action updateCallback) =>
        x =>
        {
            local.AddRange(x);

            updateCallback();
            return Task.CompletedTask;
        };

    private static Func<ItemAction<T>, Task> CreateReceive(List<T> local,
        Action updateCallback) =>
        x =>
        {
            Update(local, x);

            updateCallback();
            return Task.CompletedTask;
        };

    private static void Update(List<T> local, ItemAction<T> x)
    {
        switch (x.ActionType)
        {
            case ActionType.Add:
                local.AddRange(x.Items);
                break;
            case ActionType.Update:
                //No update in this collection
                break;
            case ActionType.Remove:
                foreach (T item in x.Items)
                {
                    local.Remove(item);
                }

                break;
            case ActionType.Clear:
                local.Clear();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
