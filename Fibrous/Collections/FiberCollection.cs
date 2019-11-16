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
        private readonly IChannel<T> _add = new Channel<T>();
        private readonly ISnapshotChannel<ItemAction<T>, T[]> _channel = new SnapshotChannel<ItemAction<T>, T[]>();
        private readonly IFiber _fiber;
        private readonly List<T> _items = new List<T>();
        private readonly IChannel<T> _remove = new Channel<T>();
        private readonly IRequestChannel<Func<T, bool>, T[]> _request = new RequestChannel<Func<T, bool>, T[]>();

        public FiberCollection(IExecutor executor = null)
        {
            _fiber = PoolFiber.StartNew(executor);
            _channel.ReplyToPrimingRequest(_fiber, Reply);
            _add.Subscribe(_fiber, AddItem);
            _remove.Subscribe(_fiber, RemoveItem);
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

        public Task<T[]> SendRequest(Func<T, bool> request)
        {
            return _request.SendRequest(request);
        }

        public IDisposable Subscribe(IFiber fiber, Action<ItemAction<T>> receive, Action<T[]> receiveSnapshot)
        {
            return _channel.Subscribe(fiber, receive, receiveSnapshot);
        }

        private void OnRequest(IRequest<Func<T, bool>, T[]> request)
        {
            request.Reply(_items.Where(request.Request).ToArray());
        }

        private void RemoveItem(T obj)
        {
            _items.Remove(obj);
            _channel.Publish(new ItemAction<T>(ActionType.Remove, obj));
        }

        private void AddItem(T obj)
        {
            _items.Add(obj);
            _channel.Publish(new ItemAction<T>(ActionType.Add, obj));
        }

        public void Add(T item)
        {
            _add.Publish(item);
        }

        public void Remove(T item)
        {
            _remove.Publish(item);
        }

        private T[] Reply()
        {
            return _items.ToArray();
        }

        public T[] GetItems(Func<T, bool> request) //, TimeSpan timout = TimeSpan.MaxValue)
        {
            return _request.SendRequest(request).Result;
        }
    }
}