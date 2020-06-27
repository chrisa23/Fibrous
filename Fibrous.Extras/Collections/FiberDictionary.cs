using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Fibrous.Collections
{
    public class FiberDictionary<TKey, T> : ISnapshotSubscriberPort<ItemAction<KeyValuePair<TKey, T>>, KeyValuePair<TKey, T>[]>,
            IRequestPort<Func<TKey, bool>, KeyValuePair<TKey, T>[]>, IDisposable
    {
        private readonly ISnapshotChannel<ItemAction<KeyValuePair<TKey, T>>, KeyValuePair<TKey, T>[]> _channel = new SnapshotChannel<ItemAction<KeyValuePair<TKey, T>>, KeyValuePair<TKey, T>[]>();
        private readonly IRequestChannel<Func<TKey, bool>, KeyValuePair<TKey, T>[]> _request = new RequestChannel<Func<TKey, bool>, KeyValuePair<TKey, T>[]>();
        private readonly IChannel<KeyValuePair<TKey, T>> _add = new Channel<KeyValuePair<TKey, T>>();
        private readonly IChannel<TKey> _remove = new Channel<TKey>();
        private readonly IFiber _fiber;

        private readonly Dictionary<TKey, T> _items = new Dictionary<TKey, T>();
        
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
        
        public void Add(TKey key, T item)
        {
            _add.Publish(new KeyValuePair<TKey, T>(key, item));
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

        public Task<Result<KeyValuePair<TKey, T>[]>> SendRequest(Func<TKey, bool> request, TimeSpan timeout)
        {
            return _request.SendRequest(request, timeout);
        }

        public IDisposable Subscribe(IFiber fiber, Action<ItemAction<KeyValuePair<TKey, T>>> receive, Action<KeyValuePair<TKey, T>[]> receiveSnapshot)
        {
            return _channel.Subscribe(fiber, receive, receiveSnapshot);
        }

        public IDisposable Subscribe(IAsyncFiber fiber, Func<ItemAction<KeyValuePair<TKey, T>>, Task> receive, Func<KeyValuePair<TKey, T>[], Task> receiveSnapshot)
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
            var data = _items.ContainsKey(obj) ? _items[obj] : default;
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

        //Helper functions to create handlers for maintaining a local collection based on a FiberDictionary
        public IDisposable SubscribeLocalCopy(IFiber fiber, Dictionary<TKey, T> localDict, Action updateCallback)
        {
            return Subscribe(fiber, CreateReceive(localDict, updateCallback), CreateSnapshot(localDict, updateCallback));
        }

        public IDisposable SubscribeLocalCopy(IAsyncFiber fiber, Dictionary<TKey, T> localDict, Action updateCallback)
        {
            return Subscribe(fiber, CreateReceiveAsync(localDict, updateCallback), CreateSnapshotAsync(localDict, updateCallback));
        }
        
        private static Action<ItemAction<KeyValuePair<TKey, T>>> CreateReceive(Dictionary<TKey, T> localDict,
            Action updateCallback)
        {
            return x =>
            {
                UpdateLocal(localDict, x);

                updateCallback();
            };
        }

        private static Action<KeyValuePair<TKey, T>[]> CreateSnapshot(Dictionary<TKey, T> localDict,
            Action updateCallback)
        {
            return x =>
            {
                foreach (var item in x)
                {
                    localDict[item.Key] = item.Value;
                }

                updateCallback();
            };
        }

        private static Func<ItemAction<KeyValuePair<TKey, T>>, Task> CreateReceiveAsync(Dictionary<TKey, T> localDict,
            Action updateCallback)
        {
            return x =>
            {
                UpdateLocal(localDict, x);

                updateCallback();
                return Task.CompletedTask;
            };
        }

        private static void UpdateLocal(Dictionary<TKey, T> localDict, ItemAction<KeyValuePair<TKey, T>> x)
        {
            switch (x.ActionType)
            {
                case ActionType.Add:
                case ActionType.Update:
                    foreach (var item in x.Items)
                    {
                        localDict[item.Key] = item.Value;
                    }

                    break;
                case ActionType.Remove:
                    foreach (var item in x.Items)
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

        private static Func<KeyValuePair<TKey, T>[], Task> CreateSnapshotAsync(Dictionary<TKey, T> localDict,
            Action updateCallback)
        {
            return x =>
            {
                foreach (var item in x)
                {
                    localDict[item.Key] = item.Value;
                }

                updateCallback();
                return Task.CompletedTask;
            };
        }
    }
}
