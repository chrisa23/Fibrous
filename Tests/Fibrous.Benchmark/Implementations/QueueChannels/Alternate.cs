using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fibrous.Benchmark.Implementations.QueueChannels
{

    /// <summary>
    ///     Queue channel where a message is consumed by only one consumer.
    /// </summary>
    /// <typeparam name="TMsg"></typeparam>
    public sealed class QueueChannel2<TMsg> : IChannel<TMsg>
    {
        private readonly ConcurrentQueue<TMsg> _queue = new ConcurrentQueue<TMsg>();
        private event Action Signal;
        public IDisposable Subscribe(IFiber fiber, Action<TMsg> onMessage)
        {
            var queueConsumer = new QueueConsumer(fiber, onMessage, this);
            return new Unsubscriber(queueConsumer, fiber);
        }

        public IDisposable Subscribe(IAsyncFiber fiber, Func<TMsg, Task> receive)
        {
            var asyncQueueConsumer = new AsyncQueueConsumer(fiber, receive, this);
            return new Unsubscriber(asyncQueueConsumer, fiber);
        }

        public IDisposable Subscribe(Action<TMsg> receive)
        {
            throw new NotImplementedException();
        }

        public void Publish(TMsg message)
        {
            _queue.Enqueue(message);
            Signal?.Invoke();

        }

        internal bool Pop(out TMsg msg)
        {
            return _queue.TryDequeue(out msg);
        }

        private interface IQueueSubscriber : IDisposable
        {
            void Signal();//TMsg msg);
        }

        public void Dispose()
        {
            Signal = null;
        }

        private sealed class QueueConsumer : IQueueSubscriber
        {
            private readonly Action<TMsg> _callback;
            private readonly QueueChannel2<TMsg> _eventChannel;
            private readonly IFiber _target;
            private readonly Action _cache;
            private int _flushPending;
            public QueueConsumer(IFiber target, Action<TMsg> callback, QueueChannel2<TMsg> eventChannel)
            {
                _target = target;
                _callback = callback;
                _eventChannel = eventChannel;
                _cache = ConsumeNext;
                _eventChannel.Signal += Signal;
            }

            public void Dispose()
            {
                _eventChannel.Signal -= Signal;
            }

            public void Signal()
            {
                if (Interlocked.CompareExchange(ref _flushPending, 1, 0) == 1) return;

                _target.Enqueue(_cache);
            }

            private void ConsumeNext()
            {
                if (_eventChannel.Pop(out var msg))
                {
                    _callback(msg);
                    _target.Enqueue(_cache);
                }
                else
                {
                    Interlocked.Exchange(ref _flushPending, 0);
                }
            }

        }

        private sealed class AsyncQueueConsumer : IQueueSubscriber
        {
            private readonly Func<TMsg, Task> _callback;
            private readonly QueueChannel2<TMsg> _eventChannel;
            private readonly IAsyncFiber _target;
            private readonly Func<Task> _cache;
            private volatile int _flushPending;

            public AsyncQueueConsumer(IAsyncFiber target, Func<TMsg, Task> callback,
                QueueChannel2<TMsg> eventChannel)
            {
                _target = target;
                _callback = callback;
                _eventChannel = eventChannel;
                _cache = ConsumeNext;
                eventChannel.Signal += Signal;
            }

            public void Dispose()
            {
                _eventChannel.Signal -= Signal;
            }

            public void Signal()
            {
                if (Interlocked.CompareExchange(ref _flushPending, 1, 0) == 1) return;

                _target.Enqueue(_cache);
            }

            private async Task ConsumeNext()
            {
                if (_eventChannel.Pop(out var msg))
                {
                    await _callback(msg);
                    _target.Enqueue(_cache);
                }
                else
                {
                    Interlocked.Exchange(ref _flushPending, 0);
                }
            }
        }

    }

    public sealed class QueueChannelRR<TMsg> : IChannel<TMsg>
    {
        private long _subCount;
        private IQueueSubscriber[] _subscribers = new IQueueSubscriber[0];
        private long _index = -1;

        private readonly object _lock = new object();
        public IDisposable Subscribe(IFiber fiber, Action<TMsg> onMessage)
        {
            var queueConsumer = new QueueConsumer(fiber, onMessage, this);
            lock (_lock)
            {
                _subscribers = _subscribers.Append(queueConsumer).ToArray();
                _subCount++;
            }
            return new Unsubscriber(queueConsumer, fiber);
        }

        public IDisposable Subscribe(IAsyncFiber fiber, Func<TMsg, Task> receive)
        {
            var asyncQueueConsumer = new AsyncQueueConsumer(fiber, receive, this);
            lock (_lock)
            {
                _subscribers = _subscribers.Append(asyncQueueConsumer).ToArray();
                _subCount++;
            }
            return new Unsubscriber(asyncQueueConsumer, fiber);
        }

        public IDisposable Subscribe(Action<TMsg> receive)
        {
            throw new NotImplementedException();
        }

        public void Publish(TMsg message)
        {
            if (_subCount == 0) return;

            long index = Interlocked.Increment(ref _index) % _subCount;

            var queueSubscriber = _subscribers[index];
            queueSubscriber.Signal(message);
        }


        private void RemoveSubscriber(IQueueSubscriber queueConsumer)
        {
            lock (_lock)
            {
                var queueSubscribers = _subscribers.ToList();
                bool found = queueSubscribers.Remove(queueConsumer);
                _subscribers = queueSubscribers.ToArray();
                _subCount--;
                //if not found?
            }
        }
        private interface IQueueSubscriber : IDisposable
        {
            void Signal(TMsg msg);
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
            private readonly QueueChannelRR<TMsg> _eventChannel;
            private readonly IFiber _target;
            public QueueConsumer(IFiber target, Action<TMsg> callback, QueueChannelRR<TMsg> eventChannel)
            {
                _target = target;
                _callback = callback;
                _eventChannel = eventChannel;
            }

            public void Dispose()
            {
                _eventChannel.RemoveSubscriber(this);
            }

            public void Signal(TMsg msg)
            {
                _target.Enqueue(() => _callback(msg));
            }
        }

        private sealed class AsyncQueueConsumer : IQueueSubscriber
        {
            private readonly Func<TMsg, Task> _callback;
            private readonly QueueChannelRR<TMsg> _eventChannel;
            private readonly IAsyncFiber _target;
            public AsyncQueueConsumer(IAsyncFiber target, Func<TMsg, Task> callback,
                QueueChannelRR<TMsg> eventChannel)
            {
                _target = target;
                _callback = callback;
                _eventChannel = eventChannel;
            }

            public void Dispose()
            {
                _eventChannel.RemoveSubscriber(this);
            }

            public void Signal(TMsg msg)
            {
                _target.Enqueue(() => _callback(msg));
            }
        }

    }


    public sealed class QueueChannelRR2<TMsg> : IChannel<TMsg>
    {
        private ConcurrentQueue<TMsg> _queue = new ConcurrentQueue<TMsg>();
        private long _subCount;
        private IQueueSubscriber[] _subscribers = new IQueueSubscriber[0];
        private long _index = -1;

        private readonly object _lock = new object();
        public IDisposable Subscribe(IFiber fiber, Action<TMsg> onMessage)
        {
            var queueConsumer = new QueueConsumer(fiber, onMessage, this);
            lock (_lock)
            {
                _subscribers = _subscribers.Append(queueConsumer).ToArray();
                _subCount++;
            }
            return new Unsubscriber(queueConsumer, fiber);
        }

        public IDisposable Subscribe(IAsyncFiber fiber, Func<TMsg, Task> receive)
        {
            var asyncQueueConsumer = new AsyncQueueConsumer(fiber, receive, this);
            lock (_lock)
            {
                _subscribers = _subscribers.Append(asyncQueueConsumer).ToArray();
                _subCount++;
            }
            return new Unsubscriber(asyncQueueConsumer, fiber);
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
            queueSubscriber.Signal();
        }


        private void RemoveSubscriber(IQueueSubscriber queueConsumer)
        {
            lock (_lock)
            {
                var queueSubscribers = _subscribers.ToList();
                bool found = queueSubscribers.Remove(queueConsumer);
                _subscribers = queueSubscribers.ToArray();
                _subCount--;
                //if not found?
            }
        }
        private interface IQueueSubscriber : IDisposable
        {
            void Signal();
        }
        internal bool Pop(out TMsg msg)
        {
            return _queue.TryDequeue(out msg);
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
            private readonly QueueChannelRR2<TMsg> _eventChannel;
            private readonly IFiber _target;
            private readonly Action _cache;
            private int _flushPending;
            public QueueConsumer(IFiber target, Action<TMsg> callback, QueueChannelRR2<TMsg> eventChannel)
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
                if (Interlocked.CompareExchange(ref _flushPending, 1, 0) == 1) return;

                _target.Enqueue(_cache);
            }

            private void ConsumeNext()
            {
                if (_eventChannel.Pop(out var msg))
                {
                    _callback(msg);
                    _target.Enqueue(_cache);
                }
                else
                {
                    Interlocked.Exchange(ref _flushPending, 0);
                }
            }

        }

        private sealed class AsyncQueueConsumer : IQueueSubscriber
        {
            private readonly Func<TMsg, Task> _callback;
            private readonly QueueChannelRR2<TMsg> _eventChannel;
            private readonly IAsyncFiber _target;
            private readonly Func<Task> _cache;
            private volatile int _flushPending;

            public AsyncQueueConsumer(IAsyncFiber target, Func<TMsg, Task> callback,
                QueueChannelRR2<TMsg> eventChannel)
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
                if (Interlocked.CompareExchange(ref _flushPending, 1, 0) == 1) return;

                _target.Enqueue(_cache);
            }

            private async Task ConsumeNext()
            {
                if (_eventChannel.Pop(out var msg))
                {
                    await _callback(msg);
                    _target.Enqueue(_cache);
                }
                else
                {
                    Interlocked.Exchange(ref _flushPending, 0);
                }
            }
        }

    }


    public sealed class QueueChannelRR3<TMsg> : IChannel<TMsg>
    {
        private ConcurrentQueue<TMsg> _queue = new ConcurrentQueue<TMsg>();
        private long _subCount;
        private IQueueSubscriber[] _subscribers = new IQueueSubscriber[0];
        private long _index = -1;

        private readonly object _lock = new object();
        public IDisposable Subscribe(IFiber fiber, Action<TMsg> onMessage)
        {
            var queueConsumer = new QueueConsumer(fiber, onMessage, this);
            lock (_lock)
            {
                _subscribers = _subscribers.Append(queueConsumer).ToArray();
                _subCount++;
            }
            return new Unsubscriber(queueConsumer, fiber);
        }

        public IDisposable Subscribe(IAsyncFiber fiber, Func<TMsg, Task> receive)
        {
            var asyncQueueConsumer = new AsyncQueueConsumer(fiber, receive, this);
            lock (_lock)
            {
                _subscribers = _subscribers.Append(asyncQueueConsumer).ToArray();
                _subCount++;
            }
            return new Unsubscriber(asyncQueueConsumer, fiber);
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
            queueSubscriber.Signal();
        }


        private void RemoveSubscriber(IQueueSubscriber queueConsumer)
        {
            lock (_lock)
            {
                var queueSubscribers = _subscribers.ToList();
                bool found = queueSubscribers.Remove(queueConsumer);
                _subscribers = queueSubscribers.ToArray();
                _subCount--;
                //if not found?
            }
        }
        private interface IQueueSubscriber : IDisposable
        {
            void Signal();
        }
        internal bool Pop(out TMsg msg)
        {
            return _queue.TryDequeue(out msg);
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

        private int Count => _queue.Count;

        private sealed class QueueConsumer : IQueueSubscriber
        {
            private readonly Action<TMsg> _callback;
            private readonly QueueChannelRR3<TMsg> _eventChannel;
            private readonly IFiber _target;
            private readonly Action _cache;
            private bool _flushPending;

            public QueueConsumer(IFiber target, Action<TMsg> callback, QueueChannelRR3<TMsg> eventChannel)
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
                lock (this)
                {
                    if (_flushPending) return;
                    _flushPending = true;
                    _target.Enqueue(_cache);
                }
            }

            private void ConsumeNext()
            {
                if (_eventChannel.Pop(out var msg))
                {
                    _callback(msg);

                }

                lock (this)
                {
                    if (_eventChannel.Count == 0)
                        _flushPending = false;
                    else
                        _target.Enqueue(_cache);
                }

            }

        }

        private sealed class AsyncQueueConsumer : IQueueSubscriber
        {
            private readonly Func<TMsg, Task> _callback;
            private readonly QueueChannelRR3<TMsg> _eventChannel;
            private readonly IAsyncFiber _target;
            private readonly Func<Task> _cache;
            private volatile int _flushPending;

            public AsyncQueueConsumer(IAsyncFiber target, Func<TMsg, Task> callback,
                QueueChannelRR3<TMsg> eventChannel)
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
                if (Interlocked.CompareExchange(ref _flushPending, 1, 0) == 1) return;

                _target.Enqueue(_cache);
            }

            private async Task ConsumeNext()
            {
                if (_eventChannel.Pop(out var msg))
                {
                    await _callback(msg);
                    _target.Enqueue(_cache);
                }
                else
                {
                    Interlocked.Exchange(ref _flushPending, 0);
                }
            }
        }

    }
}
