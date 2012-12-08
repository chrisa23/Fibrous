namespace Fibrous
{
    using System;
    using System.Collections.Generic;

    public sealed class QueueChannel<TMsg> : IChannel<TMsg>
    {
        private readonly Queue<TMsg> _queue = new Queue<TMsg>();
        private int Count
        {
            get
            {
                lock (_queue)
                {
                    return _queue.Count;
                }
            }
        }

        public IDisposable Subscribe(Fiber fiber, Action<TMsg> onMessage)
        {
            return new QueueConsumer(fiber, onMessage, this);
        }

        public bool Publish(TMsg message)
        {
            lock (_queue)
            {
                _queue.Enqueue(message);
            }
            Action onSignal = SignalEvent;
            if (onSignal != null)
                onSignal();
            return true;
        }

        internal event Action SignalEvent;

        private bool Pop(out TMsg msg)
        {
            lock (_queue)
            {
                if (_queue.Count > 0)
                {
                    msg = _queue.Dequeue();
                    return true;
                }
            }
            msg = default(TMsg);
            return false;
        }

        private sealed class QueueConsumer : IDisposable
        {
            private readonly Action<TMsg> _callback;
            private readonly QueueChannel<TMsg> _eventChannel;
            private readonly IExecutionContext _target;
            private bool _flushPending;

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
                lock (this)
                {
                    if (_flushPending)
                        return;
                    _target.Enqueue(ConsumeNext);
                    _flushPending = true;
                }
            }

            private void ConsumeNext()
            {
                try
                {
                    TMsg msg;
                    if (_eventChannel.Pop(out msg))
                        _callback(msg);
                }
                finally
                {
                    lock (this)
                    {
                        if (_eventChannel.Count == 0)
                            _flushPending = false;
                        else
                            _target.Enqueue(ConsumeNext);
                    }
                }
            }
        }
    }
}