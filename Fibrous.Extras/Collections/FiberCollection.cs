using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fibrous.Collections
{
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
        private readonly List<T> _items = new List<T>();
        private readonly IRequestChannel<Func<T, bool>, T[]> _request = new RequestChannel<Func<T, bool>, T[]>();

        public FiberCollection(IExecutor executor = null)
        {
            _fiber = new Fiber(executor);
            _channel.ReplyToPrimingRequest(_fiber, Reply);
            _request.SetRequestHandler(_fiber, OnRequest);
        }

        public void Dispose()
        {
            _fiber.Dispose();
        }

        public IDisposable SendRequest(Func<T, bool> request, IFiber fiber, Action<T[]> onReply)
        {
            return _request.SendRequest(request, fiber, onReply);
        }

        public IDisposable SendRequest(Func<T, bool> request, IAsyncFiber fiber, Func<T[], Task> onReply)
        {
            return _request.SendRequest(request, fiber, onReply);
        }

        public Task<T[]> SendRequest(Func<T, bool> request)
        {
            return _request.SendRequest(request);
        }

        public Task<Result<T[]>> SendRequest(Func<T, bool> request, TimeSpan timeout)
        {
            return _request.SendRequest(request, timeout);
        }

        public IDisposable Subscribe(IFiber fiber, Action<ItemAction<T>> receive, Action<T[]> receiveSnapshot)
        {
            return _channel.Subscribe(fiber, receive, receiveSnapshot);
        }

        public IDisposable Subscribe(IAsyncFiber fiber, Func<ItemAction<T>, Task> receive, Func<T[], Task> receiveSnapshot)
        {
            return _channel.Subscribe(fiber, receive, receiveSnapshot);
        }

        public void Clear()
        {
            _fiber.Enqueue(() =>
            {
                _items.Clear();
                _channel.Publish(new ItemAction<T>(ActionType.Clear, new T[] { }));
            });
        }

        public void AddRange(IEnumerable<T> items)
        {
            _fiber.Enqueue(() =>
            {
                var itemArray = items.ToArray();
                _items.AddRange(itemArray);
                _channel.Publish(new ItemAction<T>(ActionType.Add, itemArray));
            });
        }

        public void Add(T item)
        {
            _fiber.Enqueue(() =>
            {
                _items.Add(item);
                _channel.Publish(new ItemAction<T>(ActionType.Add, new[] {item}));
            });
        }

        public void Remove(T item)
        {
            _fiber.Enqueue(() =>
            {
                var removed = _items.Remove(item);
                if (removed)
                    _channel.Publish(new ItemAction<T>(ActionType.Remove, new[] {item}));
            });
        }

        public T[] GetItems(Func<T, bool> request) //, TimeSpan timeout = TimeSpan.MaxValue)
        {
            return _request.SendRequest(request).Result;
        }

        private void OnRequest(IRequest<Func<T, bool>, T[]> request)
        {
            request.Reply(_items.Where(request.Request).ToArray());
        }

        private T[] Reply()
        {
            return _items.ToArray();
        }

        //Helper functions to create handlers for maintaining a local collection based on a FiberDictionary
        public IDisposable SubscribeLocalCopy(IFiber fiber, List<T> local, Action updateCallback)
        {
            return Subscribe(fiber, CreateReceive(local, updateCallback), CreateSnapshot(local, updateCallback));
        }

        public IDisposable SubscribeLocalCopy(IAsyncFiber fiber, List<T> local, Action updateCallback)
        {
            return Subscribe(fiber, CreateReceiveAsync(local, updateCallback), CreateSnapshotAsync(local, updateCallback));
        }

        public static Action<ItemAction<T>> CreateReceive(List<T> local, Action updateCallback)
        {
            return x =>
            {
                Update(local, x);

                updateCallback();
            };
        }

        private static Action<T[]> CreateSnapshot(List<T> local ,
            Action updateCallback)
        {
            return x =>
            {
                local.AddRange(x);

                updateCallback();
            };
        }

        private static Func<T[], Task> CreateSnapshotAsync(List<T> local,
            Action updateCallback)
        {
            return x =>
            {
                local.AddRange(x);

                updateCallback();
                return Task.CompletedTask;
            };
        }
        
        private static Func<ItemAction<T>, Task> CreateReceiveAsync(List<T> local,
            Action updateCallback)
        {
            return x =>
            {
                Update(local, x);

                updateCallback();
                return Task.CompletedTask;
            };
        }

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
                    foreach (var item in x.Items)
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
}