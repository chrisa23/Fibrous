using System;
using System.Collections.Concurrent;
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

        public IDisposable Subscribe(IFiber fiber, Action<TMsg> onMessage)
        {
            return new QueueConsumer(fiber, onMessage, this);
        }

        public IDisposable Subscribe(IAsyncFiber fiber, Func<TMsg, Task> receive)
        {
            return new AsyncQueueConsumer(fiber, receive, this);
        }

        public void Publish(TMsg message)
        {
            _queue.Enqueue(message);
            var onSignal = SignalEvent;
            onSignal?.Invoke();
        }

        internal event Action SignalEvent;

        internal bool Pop(out TMsg msg)
        {
            return _queue.TryDequeue(out msg);
        }

        private sealed class QueueConsumer : IDisposable
        {
            private readonly Action<TMsg> _callback;
            private readonly QueueChannel<TMsg> _eventChannel;
            private readonly IExecutionContext _target;

            public QueueConsumer(IExecutionContext target, Action<TMsg> callback, QueueChannel<TMsg> eventChannel)
            {
                _target = target;
                _callback = callback;
                _eventChannel = eventChannel;
                _eventChannel.SignalEvent += Signal;
            }

            public void Dispose()
            {
                _eventChannel.SignalEvent -= Signal;
            }

            private void Signal()
            {
                if (_eventChannel.Pop(out var msg))
                    _target.Enqueue(() => _callback(msg));
            }
        }

        private sealed class AsyncQueueConsumer : IDisposable
        {
            private readonly Func<TMsg, Task> _callback;
            private readonly QueueChannel<TMsg> _eventChannel;
            private readonly IAsyncExecutionContext _target;

            public AsyncQueueConsumer(IAsyncExecutionContext target, Func<TMsg, Task> callback,
                QueueChannel<TMsg> eventChannel)
            {
                _target = target;
                _callback = callback;
                _eventChannel = eventChannel;
                _eventChannel.SignalEvent += Signal;
            }

            public void Dispose()
            {
                _eventChannel.SignalEvent -= Signal;
            }

            private void Signal()
            {
                if (_eventChannel.Pop(out var msg))
                    _target.Enqueue(() => _callback(msg));
            }
        }
    }
}