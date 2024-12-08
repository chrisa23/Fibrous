using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fibrous;

/// <summary>
///     Queue channel where a message is consumed by only one consumer.
/// </summary>
/// <typeparam name="TMsg"></typeparam>
public sealed class QueueChannel<TMsg> : IChannel<TMsg>
{
    private readonly object _lock = new();
    private readonly ConcurrentQueue<TMsg> _queue = new();

    private long _index = -1;
    private long _subCount;
    private IQueueSubscriber[] _subscribers = [];

    public IDisposable Subscribe(IFiber fiber, Func<TMsg, Task> receive)
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
        lock (_lock)
        {
            long index = Interlocked.Increment(ref _index) % _subCount;

            IQueueSubscriber queueSubscriber = _subscribers[index];
            queueSubscriber.Signal();
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

            _subscribers = [];
            _subCount = 0;
        }
    }

    private bool Pop(out TMsg msg) => _queue.TryDequeue(out msg);

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

    private sealed class AsyncQueueConsumer : IQueueSubscriber
    {
        private readonly Func<Task> _cache;
        private readonly Func<TMsg, Task> _callback;
        private readonly QueueChannel<TMsg> _eventChannel;
        private readonly IFiber _target;

        public AsyncQueueConsumer(IFiber target, Func<TMsg, Task> callback,
            QueueChannel<TMsg> eventChannel)
        {
            _target = target;
            _callback = callback;
            _eventChannel = eventChannel;
            _cache = ConsumeNextAsync;
        }

        public void Dispose() => _eventChannel.RemoveSubscriber(this);

        public void Signal() => _target.Enqueue(_cache);

        private async Task ConsumeNextAsync()
        {
            if (_eventChannel.Pop(out TMsg msg))
            {
                await _callback(msg);
            }
        }
    }
}
