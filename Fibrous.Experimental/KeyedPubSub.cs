using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fibrous
{
    public interface IKeyedPubSub<TKey, T, TSnapshot>
    {
        IDisposable Subscribe(IFiber fiber, TKey key, Action<(TKey Key, T Item)> receive, Action<(TKey Key, TSnapshot Snapshot)> snapshot);
    }

    public abstract class KeyedPubSub<TKey, T, TSnapshot> : AsyncFiberComponent, IKeyedPubSub<TKey, T, TSnapshot>, IPublisherPort<(TKey Key, T Item)>
    {
        private readonly IChannel<Subscription> _subscribe;
        private readonly IChannel<TKey> _unsubscribe;

        private readonly Dictionary<TKey, int> _subCount = new Dictionary<TKey, int>();
        private readonly Dictionary<TKey, IChannel<(TKey key, T item)>> _channels = new Dictionary<TKey, IChannel<(TKey key, T item)>>();


        protected KeyedPubSub()
        {
            _subscribe = Fiber.NewChannel<Subscription>(OnSubscription);
            _unsubscribe = Fiber.NewChannel<TKey>(OnUnsubscribe);
        }

        private async Task OnSubscription(Subscription obj)
        {
            bool newSubscription = false;
            if (!_subCount.ContainsKey(obj.Key))
            {
                _subCount[obj.Key] = 0;
                _channels[obj.Key] = new Channel<(TKey key, T item)>();
                newSubscription = true;
            }

            _subCount[obj.Key]++;
            var channel = _channels[obj.Key];
            obj.Unsubscribe = channel.Subscribe(obj.Fiber, obj.Receive);
            //Do snapshot...
            await Subscribe(newSubscription, obj.Key, x => obj.Fiber.Enqueue(() => obj.Snapshot(x)));
        }

        //deal with caching for snapshot on sub
        protected abstract Task HandlePublished((TKey key, T item) update);
        //know when new subscription and and able to send a snapshot
        // - problem is, this is possibly blocking on new subscriptions depending on how they are initialized
        protected abstract Task Subscribe(bool newSubscription, TKey key, Action<(TKey key, TSnapshot item)> action);
        //When all subscribers for the key are gone
        protected abstract Task Unsubscribe(TKey key);

        public void Publish((TKey Key, T Item) update)
        {
            Fiber.Enqueue(async () =>
            {
                await HandlePublished(update);
                if (_channels.ContainsKey(update.Key))
                    _channels[update.Key].Publish(update);
            });
        }
        
        private async Task OnUnsubscribe(TKey obj)
        {
            _subCount[obj]--;
            if (_subCount[obj] == 0)
            {
                _subCount.Remove(obj);
                _channels[obj].Dispose();
                _channels.Remove(obj);
                await Unsubscribe(obj);
            }
        }
        
        public IDisposable Subscribe(IFiber fiber, TKey key, Action<(TKey Key, T Item)> receive, Action<(TKey Key, TSnapshot Snapshot)> snapshot)
        {
            var sub = new Subscription(fiber, key, receive, snapshot, _unsubscribe);
            _subscribe.Publish(sub);
            return sub;
        }

        private class Subscription:IDisposable
        {
            internal TKey Key { get; }
            internal Action<(TKey key, T item)> Receive { get; private set; }
            internal Action<(TKey key, TSnapshot item)> Snapshot{ get; private set; }
            internal IFiber Fiber { get; private set; }
            private IChannel<TKey> _unsubscribe;
         
            internal IDisposable Unsubscribe;
            public Subscription(IFiber fiber, TKey key, Action<(TKey key, T item)> receive,
                Action<(TKey key, TSnapshot item)> snapshot, IChannel<TKey> unsubscribe)
            {
                Key = key;
                Receive = receive;
                Fiber = fiber;
                Snapshot = snapshot;
                _unsubscribe = unsubscribe;
            }

            public void Dispose()
            {
                //send a signal back
                Fiber = null;
                Receive = null;
                Snapshot = null;
                Unsubscribe?.Dispose();
                _unsubscribe.Publish(Key);
                _unsubscribe = null;
            }
        }
    }

    public class SampleKeyedPubSub:KeyedPubSub<string, int, int>
    {
        private Dictionary<string, int> _cache = new Dictionary<string, int>();
        protected override void OnError(Exception obj)
        {
            
        }

        protected override Task HandlePublished((string key, int item) update)
        {
            _cache[update.key] = update.item;
            return Task.CompletedTask;
        }

        protected override Task Subscribe(bool newSubscription, string key, Action<(string key, int item)> action)
        {
            if (newSubscription)
            {
                //internal subscribe
            }

            if (_cache.ContainsKey(key))
            {
                action((key,_cache[key]));
            }
            return Task.CompletedTask;
        }

        protected override Task Unsubscribe(string key)
        {
            _cache.Remove(key);
            //internal unsubscribe//
            return Task.CompletedTask;
        }
    }
}
