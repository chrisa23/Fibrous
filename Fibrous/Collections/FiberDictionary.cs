using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fibrous.Collections
{
    public class FiberDictionary<TKey, T> : ISnapshotSubscriberPort<ItemAction<KeyValuePair<TKey, T>>, KeyValuePair<TKey, T>[]>,
            IRequestPort<Func<TKey, bool>, KeyValuePair<TKey, T>[]>, IDisposable
    {
        private readonly IChannel<KeyValuePair<TKey, T>> _add = new Channel<KeyValuePair<TKey, T>>();
        private readonly ISnapshotChannel<ItemAction<KeyValuePair<TKey, T>>, KeyValuePair<TKey, T>[]> _channel = new SnapshotChannel<ItemAction<KeyValuePair<TKey, T>>, KeyValuePair<TKey, T>[]>();
        private readonly IFiber _fiber;
        private readonly Dictionary<TKey, T> _items = new Dictionary<TKey, T>();

        private readonly IChannel<TKey> _remove = new Channel<TKey>();
        private readonly IRequestChannel<Func<TKey, bool>, KeyValuePair<TKey, T>[]> _request = new RequestChannel<Func<TKey, bool>, KeyValuePair<TKey, T>[]>();

        public FiberDictionary(IExecutor executor = null)
        {
            _fiber = new Fiber(executor);
            _channel.ReplyToPrimingRequest(_fiber, Reply);
            _add.Subscribe(_fiber, AddItem);
            _remove.Subscribe(_fiber, RemoveItem);
            _request.SetRequestHandler(_fiber, OnRequest);
        }

        public void Dispose()
        {
            _fiber.Dispose();
        }
        public void Add(KeyValuePair<TKey, T> item)
        {
            _add.Publish(item);
        }

        public void Remove(TKey item)
        {
            _remove.Publish(item);
        }

        public void Clear()
        {
            _fiber.Enqueue(() =>
            {
                _items.Clear();
                _channel.Publish(new ItemAction<KeyValuePair<TKey, T>>(ActionType.Clear, new KeyValuePair<TKey, T>[0]));
            });
        }

        public void AddRange(IEnumerable<KeyValuePair<TKey, T>> items)
        {
            _fiber.Enqueue(() =>
            {
                var added = new List<KeyValuePair<TKey, T>>();
                var updated = new List<KeyValuePair<TKey, T>>();
                foreach (var item in items)
                {
                    if (_items.ContainsKey(item.Key))
                        updated.Add(item);
                    else
                        added.Add(item);
                    _items[item.Key] = item.Value;
                }
                if(added.Count > 0)
                    _channel.Publish(new ItemAction<KeyValuePair<TKey, T>>(ActionType.Add, added.ToArray()));
                if (updated.Count > 0)
                    _channel.Publish(new ItemAction<KeyValuePair<TKey, T>>(ActionType.Update, updated.ToArray()));
            });
        }

        public IDisposable SendRequest(Func<TKey, bool> request, IFiber fiber, Action<KeyValuePair<TKey, T>[]> onReply)
        {
            return _request.SendRequest(request, fiber, onReply);
        }

        public IDisposable SendRequest(Func<TKey, bool> request, IAsyncFiber fiber, Func<KeyValuePair<TKey, T>[], Task> onReply)
        {
            return _request.SendRequest(request, fiber, onReply);
        }

        public Task<KeyValuePair<TKey, T>[]> SendRequest(Func<TKey, bool> request)
        {
            return _request.SendRequest(request);
        }

        public IDisposable Subscribe(IFiber fiber, Action<ItemAction<KeyValuePair<TKey, T>>> receive, Action<KeyValuePair<TKey, T>[]> receiveSnapshot)
        {
            return _channel.Subscribe(fiber, receive, receiveSnapshot);
        }


        public KeyValuePair<TKey, T>[] GetItems(Func<TKey, bool> request)
        {
            return _request.SendRequest(request).Result;
        }
        public async Task<KeyValuePair<TKey, T>[]> GetItemsAsync(Func<TKey, bool> request)
        {
            return await _request.SendRequest(request);
        }

        private void OnRequest(IRequest<Func<TKey, bool>, KeyValuePair<TKey, T>[]> request)
        {
            request.Reply(_items.Where(x => request.Request(x.Key)).ToArray());
        }

        private void RemoveItem(TKey obj)
        {
            var data = _items.ContainsKey(obj) ? _items[obj] : default(T);
            var removed = _items.Remove(obj);
            if (removed)
                _channel.Publish(new ItemAction<KeyValuePair<TKey, T>>(ActionType.Remove, new []{ new KeyValuePair<TKey, T>(obj, data)}));
        }

        private void AddItem(KeyValuePair<TKey, T> obj)
        {
            var exists = _items.ContainsKey(obj.Key);
            _items[obj.Key] = obj.Value;
            _channel.Publish(new ItemAction<KeyValuePair<TKey, T>>(exists ? ActionType.Update : ActionType.Add, new[] { obj}));
        }

        private KeyValuePair<TKey, T>[] Reply()
        {
            return _items.ToArray();
        }
    }
}
