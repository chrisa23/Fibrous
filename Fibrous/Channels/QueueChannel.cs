using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Fibrous
{
    /// <summary>
    ///     Queue channel where a message is consumed by only one consumer.
    /// </summary>
    /// <typeparam name="TMsg"></typeparam>
    public sealed class QueueChannel<TMsg> : IChannel<TMsg>
    {
        private readonly ConcurrentQueue<TMsg> _queue = new ConcurrentQueue<TMsg>();
        private long _subCount;
        private IQueueSubscriber[] _subscribers =new IQueueSubscriber[0];

        private long _index =  -1;
        
        private readonly object _lock = new object();
        public IDisposable Subscribe(IFiber fiber, Action<TMsg> onMessage)
        {
            var queueConsumer = new QueueConsumer(fiber, onMessage, this);
            lock (_lock)
            {
                _subscribers = _subscribers.Append(queueConsumer).ToArray();
                _subCount++;
            }
            return new Unsubscriber( queueConsumer, fiber);
        }

        public IDisposable Subscribe(IAsyncFiber fiber, Func<TMsg, Task> receive)
        {
            var asyncQueueConsumer = new AsyncQueueConsumer(fiber, receive, this);
            lock (_lock)
            {
                _subscribers = _subscribers.Append(asyncQueueConsumer).ToArray();
                _subCount++;
            }
            return new Unsubscriber( asyncQueueConsumer, fiber);
        }

        public IDisposable Subscribe(Action<TMsg> receive)
        {
            throw new NotImplementedException();
        }

        public void Publish(TMsg message)
        {
            if (_subCount == 0) return;

            _queue.Enqueue(message);
            long index = Interlocked.Increment(ref _index) % _subCount;

            var queueSubscriber = _subscribers[index];
            queueSubscriber.Signal();//
        }

        internal bool Pop(out TMsg msg)
        {
            return _queue.TryDequeue(out msg);
        }
        private void RemoveSubscriber(IQueueSubscriber queueConsumer)
        {
            lock (_lock)
            {
                var queueSubscribers = _subscribers.ToList();
                bool found = queueSubscribers.Remove(queueConsumer);
                _subscribers = queueSubscribers.ToArray();
                if(found)
                    _subCount--;
            }
        }
        private interface IQueueSubscriber : IDisposable
        {
            void Signal();
        }

        public void Dispose()
        {
            lock (_lock)
            {
                foreach (var subscriber in _subscribers)
                {
                    subscriber.Dispose();
                }
                _subscribers = new IQueueSubscriber[0];
                _subCount = 0;
            }
        }

        private sealed class QueueConsumer : IQueueSubscriber
        {
            private readonly Action<TMsg> _callback;
            private readonly QueueChannel<TMsg> _eventChannel;
            private readonly IFiber _target;
            private readonly Action _cache;
            public QueueConsumer(IFiber target, Action<TMsg> callback, QueueChannel<TMsg> eventChannel)
            {
                _target = target;
                _callback = callback;
                _eventChannel = eventChannel;
                _cache = ConsumeNext;
            }

            public void Dispose()
            {
                _eventChannel.RemoveSubscriber(this);
            }

            public void Signal()
            {
                _target.Enqueue(_cache);
            }

            private void ConsumeNext()
            {
                if (_eventChannel.Pop(out var msg))
                {
                    _callback(msg);
                }
            }
        }

        private sealed class AsyncQueueConsumer : IQueueSubscriber
        {
            private readonly Func<TMsg, Task> _callback;
            private readonly QueueChannel<TMsg> _eventChannel;
            private readonly IAsyncFiber _target;
            private readonly Func<Task> _cache;
            public AsyncQueueConsumer(IAsyncFiber target, Func<TMsg, Task> callback,
                QueueChannel<TMsg> eventChannel)
            {
                _target = target;
                _callback = callback;
                _eventChannel = eventChannel;
                _cache = ConsumeNext;
            }

            public void Dispose()
            {
                _eventChannel.RemoveSubscriber(this);
            }

            public void Signal()
            {
                _target.Enqueue(_cache);
            }

            private async Task ConsumeNext()
            {
                if (_eventChannel.Pop(out var msg))
                {
                    await _callback(msg);
                }
            }
        }

    }
}