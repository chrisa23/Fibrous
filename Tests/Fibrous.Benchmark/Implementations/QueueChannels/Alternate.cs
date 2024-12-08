﻿/*
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
        private readonly ConcurrentQueue<TMsg> _queue = new();


        public IDisposable Subscribe(IAsyncFiber fiber, Func<TMsg, Task> receive)
        {
            AsyncQueueConsumer asyncQueueConsumer = new(fiber, receive, this);
            return new Unsubscriber(asyncQueueConsumer, fiber);
        }

        public IDisposable Subscribe(Action<TMsg> receive) => throw new NotImplementedException();

        public void Publish(TMsg message)
        {
            _queue.Enqueue(message);
            Signal?.Invoke();
        }

        public void Dispose() => Signal = null;

        private event Action Signal;

        internal bool Pop(out TMsg msg) => _queue.TryDequeue(out msg);

        private interface IQueueSubscriber : IDisposable
        {
            void Signal(); //TMsg msg);
        }

        private sealed class QueueConsumer : IQueueSubscriber
        {
            private readonly Action _cache;
            private readonly Action<TMsg> _callback;
            private readonly QueueChannel2<TMsg> _eventChannel;
            private readonly IFiber _target;
            private int _flushPending;

            public QueueConsumer(IFiber target, Action<TMsg> callback, QueueChannel2<TMsg> eventChannel)
            {
                _target = target;
                _callback = callback;
                _eventChannel = eventChannel;
                _cache = ConsumeNext;
                _eventChannel.Signal += Signal;
            }

            public void Dispose() => _eventChannel.Signal -= Signal;

            public void Signal()
            {
                if (Interlocked.CompareExchange(ref _flushPending, 1, 0) == 1)
                {
                    return;
                }

                _target.Enqueue(_cache);
            }

            private void ConsumeNext()
            {
                if (_eventChannel.Pop(out TMsg msg))
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
            private readonly Func<Task> _cache;
            private readonly Func<TMsg, Task> _callback;
            private readonly QueueChannel2<TMsg> _eventChannel;
            private readonly IAsyncFiber _target;
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

            public void Dispose() => _eventChannel.Signal -= Signal;

            public void Signal()
            {
                if (Interlocked.CompareExchange(ref _flushPending, 1, 0) == 1)
                {
                    return;
                }

                _target.Enqueue(_cache);
            }

            private async Task ConsumeNext()
            {
                if (_eventChannel.Pop(out TMsg msg))
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
        private readonly object _lock = new();
        private long _index = -1;
        private long _subCount;
        private IQueueSubscriber[] _subscribers = new IQueueSubscriber[0];

        public IDisposable Subscribe(IFiber fiber, Action<TMsg> onMessage)
        {
            QueueConsumer queueConsumer = new(fiber, onMessage, this);
            lock (_lock)
            {
                _subscribers = _subscribers.Append(queueConsumer).ToArray();
                _subCount++;
            }

            return new Unsubscriber(queueConsumer, fiber);
        }

        public IDisposable Subscribe(IAsyncFiber fiber, Func<TMsg, Task> receive)
        {
            AsyncQueueConsumer asyncQueueConsumer = new(fiber, receive, this);
            lock (_lock)
            {
                _subscribers = _subscribers.Append(asyncQueueConsumer).ToArray();
                _subCount++;
            }

            return new Unsubscriber(asyncQueueConsumer, fiber);
        }

        public IDisposable Subscribe(Action<TMsg> receive) => throw new NotImplementedException();

        public void Publish(TMsg message)
        {
            if (_subCount == 0)
            {
                return;
            }

            long index = Interlocked.Increment(ref _index) % _subCount;

            IQueueSubscriber queueSubscriber = _subscribers[index];
            queueSubscriber.Signal(message);
        }

        public void Dispose()
        {
            lock (_lock)
            {
                foreach (IQueueSubscriber subscriber in _subscribers)
                {
                    subscriber.Dispose();
                }

                _subscribers = new IQueueSubscriber[0];
                _subCount = 0;
            }
        }


        private void RemoveSubscriber(IQueueSubscriber queueConsumer)
        {
            lock (_lock)
            {
                List<IQueueSubscriber> queueSubscribers = _subscribers.ToList();
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

            public void Dispose() => _eventChannel.RemoveSubscriber(this);

            public void Signal(TMsg msg) => _target.Enqueue(() => _callback(msg));
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

            public void Dispose() => _eventChannel.RemoveSubscriber(this);

            public void Signal(TMsg msg) => _target.Enqueue(() => _callback(msg));
        }
    }


    public sealed class QueueChannelRR2<TMsg> : IChannel<TMsg>
    {
        private readonly object _lock = new();
        private readonly ConcurrentQueue<TMsg> _queue = new();
        private long _index = -1;
        private long _subCount;
        private IQueueSubscriber[] _subscribers = new IQueueSubscriber[0];

        public IDisposable Subscribe(IFiber fiber, Action<TMsg> onMessage)
        {
            QueueConsumer queueConsumer = new(fiber, onMessage, this);
            lock (_lock)
            {
                _subscribers = _subscribers.Append(queueConsumer).ToArray();
                _subCount++;
            }

            return new Unsubscriber(queueConsumer, fiber);
        }

        public IDisposable Subscribe(IAsyncFiber fiber, Func<TMsg, Task> receive)
        {
            AsyncQueueConsumer asyncQueueConsumer = new(fiber, receive, this);
            lock (_lock)
            {
                _subscribers = _subscribers.Append(asyncQueueConsumer).ToArray();
                _subCount++;
            }

            return new Unsubscriber(asyncQueueConsumer, fiber);
        }

        public IDisposable Subscribe(Action<TMsg> receive) => throw new NotImplementedException();

        public void Publish(TMsg message)
        {
            if (_subCount == 0)
            {
                return;
            }

            _queue.Enqueue(message);
            long index = Interlocked.Increment(ref _index) % _subCount;

            IQueueSubscriber queueSubscriber = _subscribers[index];
            queueSubscriber.Signal();
        }

        public void Dispose()
        {
            lock (_lock)
            {
                foreach (IQueueSubscriber subscriber in _subscribers)
                {
                    subscriber.Dispose();
                }

                _subscribers = new IQueueSubscriber[0];
                _subCount = 0;
            }
        }


        private void RemoveSubscriber(IQueueSubscriber queueConsumer)
        {
            lock (_lock)
            {
                List<IQueueSubscriber> queueSubscribers = _subscribers.ToList();
                bool found = queueSubscribers.Remove(queueConsumer);
                _subscribers = queueSubscribers.ToArray();
                _subCount--;
                //if not found?
            }
        }

        internal bool Pop(out TMsg msg) => _queue.TryDequeue(out msg);

        private interface IQueueSubscriber : IDisposable
        {
            void Signal();
        }

        private sealed class QueueConsumer : IQueueSubscriber
        {
            private readonly Action _cache;
            private readonly Action<TMsg> _callback;
            private readonly QueueChannelRR2<TMsg> _eventChannel;
            private readonly IFiber _target;
            private int _flushPending;

            public QueueConsumer(IFiber target, Action<TMsg> callback, QueueChannelRR2<TMsg> eventChannel)
            {
                _target = target;
                _callback = callback;
                _eventChannel = eventChannel;
                _cache = ConsumeNext;
            }

            public void Dispose() => _eventChannel.RemoveSubscriber(this);

            public void Signal()
            {
                if (Interlocked.CompareExchange(ref _flushPending, 1, 0) == 1)
                {
                    return;
                }

                _target.Enqueue(_cache);
            }

            private void ConsumeNext()
            {
                if (_eventChannel.Pop(out TMsg msg))
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
            private readonly Func<Task> _cache;
            private readonly Func<TMsg, Task> _callback;
            private readonly QueueChannelRR2<TMsg> _eventChannel;
            private readonly IAsyncFiber _target;
            private volatile int _flushPending;

            public AsyncQueueConsumer(IAsyncFiber target, Func<TMsg, Task> callback,
                QueueChannelRR2<TMsg> eventChannel)
            {
                _target = target;
                _callback = callback;
                _eventChannel = eventChannel;
                _cache = ConsumeNext;
            }

            public void Dispose() => _eventChannel.RemoveSubscriber(this);

            public void Signal()
            {
                if (Interlocked.CompareExchange(ref _flushPending, 1, 0) == 1)
                {
                    return;
                }

                _target.Enqueue(_cache);
            }

            private async Task ConsumeNext()
            {
                if (_eventChannel.Pop(out TMsg msg))
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
        private readonly object _lock = new();
        private readonly ConcurrentQueue<TMsg> _queue = new();
        private long _index = -1;
        private long _subCount;
        private IQueueSubscriber[] _subscribers = new IQueueSubscriber[0];

        private int Count => _queue.Count;

        public IDisposable Subscribe(IFiber fiber, Action<TMsg> onMessage)
        {
            QueueConsumer queueConsumer = new(fiber, onMessage, this);
            lock (_lock)
            {
                _subscribers = _subscribers.Append(queueConsumer).ToArray();
                _subCount++;
            }

            return new Unsubscriber(queueConsumer, fiber);
        }

        public IDisposable Subscribe(IAsyncFiber fiber, Func<TMsg, Task> receive)
        {
            AsyncQueueConsumer asyncQueueConsumer = new(fiber, receive, this);
            lock (_lock)
            {
                _subscribers = _subscribers.Append(asyncQueueConsumer).ToArray();
                _subCount++;
            }

            return new Unsubscriber(asyncQueueConsumer, fiber);
        }

        public IDisposable Subscribe(Action<TMsg> receive) => throw new NotImplementedException();

        public void Publish(TMsg message)
        {
            if (_subCount == 0)
            {
                return;
            }

            _queue.Enqueue(message);
            long index = Interlocked.Increment(ref _index) % _subCount;

            IQueueSubscriber queueSubscriber = _subscribers[index];
            queueSubscriber.Signal();
        }

        public void Dispose()
        {
            lock (_lock)
            {
                foreach (IQueueSubscriber subscriber in _subscribers)
                {
                    subscriber.Dispose();
                }

                _subscribers = new IQueueSubscriber[0];
                _subCount = 0;
            }
        }


        private void RemoveSubscriber(IQueueSubscriber queueConsumer)
        {
            lock (_lock)
            {
                List<IQueueSubscriber> queueSubscribers = _subscribers.ToList();
                bool found = queueSubscribers.Remove(queueConsumer);
                _subscribers = queueSubscribers.ToArray();
                _subCount--;
                //if not found?
            }
        }

        internal bool Pop(out TMsg msg) => _queue.TryDequeue(out msg);

        private interface IQueueSubscriber : IDisposable
        {
            void Signal();
        }

        private sealed class QueueConsumer : IQueueSubscriber
        {
            private readonly Action _cache;
            private readonly Action<TMsg> _callback;
            private readonly QueueChannelRR3<TMsg> _eventChannel;
            private readonly IFiber _target;
            private bool _flushPending;

            public QueueConsumer(IFiber target, Action<TMsg> callback, QueueChannelRR3<TMsg> eventChannel)
            {
                _target = target;
                _callback = callback;
                _eventChannel = eventChannel;
                _cache = ConsumeNext;
            }

            public void Dispose() => _eventChannel.RemoveSubscriber(this);

            public void Signal()
            {
                lock (this)
                {
                    if (_flushPending)
                    {
                        return;
                    }

                    _flushPending = true;
                    _target.Enqueue(_cache);
                }
            }

            private void ConsumeNext()
            {
                if (_eventChannel.Pop(out TMsg msg))
                {
                    _callback(msg);
                }

                lock (this)
                {
                    if (_eventChannel.Count == 0)
                    {
                        _flushPending = false;
                    }
                    else
                    {
                        _target.Enqueue(_cache);
                    }
                }
            }
        }

        private sealed class AsyncQueueConsumer : IQueueSubscriber
        {
            private readonly Func<Task> _cache;
            private readonly Func<TMsg, Task> _callback;
            private readonly QueueChannelRR3<TMsg> _eventChannel;
            private readonly IAsyncFiber _target;
            private volatile int _flushPending;

            public AsyncQueueConsumer(IAsyncFiber target, Func<TMsg, Task> callback,
                QueueChannelRR3<TMsg> eventChannel)
            {
                _target = target;
                _callback = callback;
                _eventChannel = eventChannel;
                _cache = ConsumeNext;
            }

            public void Dispose() => _eventChannel.RemoveSubscriber(this);

            public void Signal()
            {
                if (Interlocked.CompareExchange(ref _flushPending, 1, 0) == 1)
                {
                    return;
                }

                _target.Enqueue(_cache);
            }

            private async Task ConsumeNext()
            {
                if (_eventChannel.Pop(out TMsg msg))
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


    /// <summary>
    ///     Queue channel where a message is consumed by only one consumer.
    /// </summary>
    /// <typeparam name="TMsg"></typeparam>
    public sealed class QueueChannel3<TMsg> : IChannel<TMsg>
    {
        private readonly object _lock = new();
        private readonly ConcurrentQueue<TMsg> _queue = new();

        private long _index = -1;
        private long _subCount;
        private IQueueSubscriber[] _subscribers = new IQueueSubscriber[0];

        public IDisposable Subscribe(IFiber fiber, Action<TMsg> onMessage)
        {
            QueueConsumer queueConsumer = new(fiber, onMessage, this);
            lock (_lock)
            {
                _subscribers = _subscribers.Append(queueConsumer).ToArray();
                _subCount++;
            }

            return new Unsubscriber(queueConsumer, fiber);
        }

        public IDisposable Subscribe(IAsyncFiber fiber, Func<TMsg, Task> receive)
        {
            AsyncQueueConsumer asyncQueueConsumer = new(fiber, receive, this);
            lock (_lock)
            {
                _subscribers = _subscribers.Append(asyncQueueConsumer).ToArray();
                _subCount++;
            }

            return new Unsubscriber(asyncQueueConsumer, fiber);
        }

        public IDisposable Subscribe(Action<TMsg> receive) => throw new NotImplementedException();

        public void Publish(TMsg message)
        {
            if (_subCount == 0)
            {
                return;
            }

            _queue.Enqueue(message);
            long index = Interlocked.Increment(ref _index) % _subCount;

            for (int i = 0; i < _subCount; i++)
            {
                //equivalent of Action event but round robin starting
                _subscribers[(index + i) % _subCount].Signal();
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                foreach (IQueueSubscriber subscriber in _subscribers)
                {
                    subscriber.Dispose();
                }

                _subscribers = new IQueueSubscriber[0];
                _subCount = 0;
            }
        }

        internal bool Pop(out TMsg msg) => _queue.TryDequeue(out msg);

        private void RemoveSubscriber(IQueueSubscriber queueConsumer)
        {
            lock (_lock)
            {
                List<IQueueSubscriber> queueSubscribers = _subscribers.ToList();
                bool found = queueSubscribers.Remove(queueConsumer);
                _subscribers = queueSubscribers.ToArray();
                if (found)
                {
                    _subCount--;
                }
            }
        }

        private interface IQueueSubscriber : IDisposable
        {
            void Signal();
        }

        private sealed class QueueConsumer : IQueueSubscriber
        {
            private readonly Action _cache;
            private readonly Action<TMsg> _callback;
            private readonly QueueChannel3<TMsg> _eventChannel;
            private readonly IFiber _target;

            public QueueConsumer(IFiber target, Action<TMsg> callback, QueueChannel3<TMsg> eventChannel)
            {
                _target = target;
                _callback = callback;
                _eventChannel = eventChannel;
                _cache = ConsumeNext;
            }

            public void Dispose() => _eventChannel.RemoveSubscriber(this);

            public void Signal() => _target.Enqueue(_cache);

            private void ConsumeNext()
            {
                if (_eventChannel.Pop(out TMsg msg))
                {
                    _callback(msg);
                }
            }
        }

        private sealed class AsyncQueueConsumer : IQueueSubscriber
        {
            private readonly Func<Task> _cache;
            private readonly Func<TMsg, Task> _callback;
            private readonly QueueChannel3<TMsg> _eventChannel;
            private readonly IAsyncFiber _target;

            public AsyncQueueConsumer(IAsyncFiber target, Func<TMsg, Task> callback,
                QueueChannel3<TMsg> eventChannel)
            {
                _target = target;
                _callback = callback;
                _eventChannel = eventChannel;
                _cache = ConsumeNext;
            }

            public void Dispose() => _eventChannel.RemoveSubscriber(this);

            public void Signal() => _target.Enqueue(_cache);

            private async Task ConsumeNext()
            {
                if (_eventChannel.Pop(out TMsg msg))
                {
                    await _callback(msg);
                }
            }
        }
    }
}
*/
