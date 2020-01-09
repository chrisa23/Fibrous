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
            _fiber = PoolFiber.StartNew(executor);
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

        public IDisposable Subscribe(IFiber fiber, Action<ItemAction<T>> receive, Action<T[]> receiveSnapshot)
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
                }
            );
        }

        public T[] GetItems(Func<T, bool> request) //, TimeSpan timout = TimeSpan.MaxValue)
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
    }
}