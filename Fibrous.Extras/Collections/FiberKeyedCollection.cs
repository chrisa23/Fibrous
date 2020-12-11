using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fibrous.Collections
{
    public class FiberKeyedCollection<TKey, T> : ISnapshotSubscriberPort<ItemAction<T>, T[]>,
        IRequestPort<Func<T, bool>, T[]>, IDisposable
    {
        private readonly ISnapshotChannel<ItemAction<T>, T[]> _channel = new SnapshotChannel<ItemAction<T>, T[]>();
        private readonly IFiber _fiber;
        private readonly Dictionary<TKey, T> _items = new Dictionary<TKey, T>();
        private readonly Func<T, TKey> _keyGen;
        private readonly IRequestChannel<Func<T, bool>, T[]> _request = new RequestChannel<Func<T, bool>, T[]>();

        public FiberKeyedCollection(Func<T, TKey> keyGen, IExecutor executor = null)
        {
            _keyGen = keyGen;
            _fiber = new Fiber(executor);
            _channel.ReplyToPrimingRequest(_fiber, Reply);
            _request.SetRequestHandler(_fiber, OnRequest);
        }

        public void Dispose() => _fiber.Dispose();

        public IDisposable SendRequest(Func<T, bool> request, IFiber fiber, Action<T[]> onReply) =>
            _request.SendRequest(request, fiber, onReply);

        public IDisposable SendRequest(Func<T, bool> request, IAsyncFiber fiber, Func<T[], Task> onReply) =>
            _request.SendRequest(request, fiber, onReply);

        public Task<T[]> SendRequestAsync(Func<T, bool> request) => _request.SendRequestAsync(request);

        public Task<Result<T[]>> SendRequestAsync(Func<T, bool> request, TimeSpan timeout) =>
            _request.SendRequestAsync(request, timeout);

        public IDisposable Subscribe(IFiber fiber, Action<ItemAction<T>> receive, Action<T[]> receiveSnapshot) =>
            _channel.Subscribe(fiber, receive, receiveSnapshot);

        public IDisposable Subscribe(IAsyncFiber fiber, Func<ItemAction<T>, Task> receive,
            Func<T[], Task> receiveSnapshot) => _channel.Subscribe(fiber, receive, receiveSnapshot);

        public void Add(T item) => _fiber.Enqueue(() => AddItem(item));

        public void Remove(T item) => _fiber.Enqueue(() => RemoveItem(item));

        public T[] GetItems(Func<T, bool> request) => _request.SendRequestAsync(request).Result;

        public async Task<T[]> GetItemsAsync(Func<T, bool> request) => await _request.SendRequestAsync(request);

        private void OnRequest(IRequest<Func<T, bool>, T[]> request) =>
            request.Reply(_items.Values.Where(request.Request).ToArray());

        private void RemoveItem(T obj)
        {
            bool removed = _items.Remove(_keyGen(obj));
            if (removed)
            {
                _channel.Publish(new ItemAction<T>(ActionType.Remove, new[] {obj}));
            }
        }

        private void AddItem(T obj)
        {
            TKey key = _keyGen(obj);
            bool exists = _items.ContainsKey(key);
            _items[key] = obj;
            _channel.Publish(new ItemAction<T>(exists ? ActionType.Update : ActionType.Add, new[] {obj}));
        }

        private T[] Reply() => _items.Values.ToArray();
    }
}
