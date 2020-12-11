using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Fibrous.Extras
{
    public abstract class AsyncObserver<T> : IObserver<T>
    {
        private readonly Action _flushCache;
        private readonly object _lock = new object();
        private readonly ArrayQueue<T> _queue;
        private readonly TaskFactory _taskFactory;
        private bool _flushPending;

        public AsyncObserver(int size = QueueSize.DefaultQueueSize, TaskFactory taskFactory = null)
        {
            _queue = new ArrayQueue<T>(size);
            _taskFactory = taskFactory ??
                           new TaskFactory(TaskCreationOptions.PreferFairness, TaskContinuationOptions.None);
            _flushCache = Flush;
        }

        public abstract void OnCompleted();
        public abstract void OnError(Exception error);

        public void OnNext(T item)
        {
            AggressiveSpinWait spinWait = default;
            while (_queue.IsFull)
            {
                spinWait.SpinOnce();
            }

            lock (_lock)
            {
                _queue.Enqueue(item);

                if (_flushPending)
                {
                    return;
                }

                _flushPending = true;
                _ = _taskFactory.StartNew(_flushCache);
            }
        }

        protected abstract void Handle(T value);

        private void Flush()
        {
            (int count, T[] items) = Drain();

            for (int i = 0; i < count; i++)
            {
                T item = items[i];
                Handle(item);
            }

            lock (_lock)
            {
                if (_queue.Count > 0)
                {
                    _ = _taskFactory.StartNew(_flushCache);
                }
                else
                {
                    _flushPending = false;
                }
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private (int, T[]) Drain()
        {
            lock (_lock)
            {
                return _queue.Drain();
            }
        }
    }

    public sealed class AsyncObserverWrapper<T> : AsyncObserver<T>
    {
        private readonly IObserver<T> _wrapped;

        public AsyncObserverWrapper(IObserver<T> wrapped) => _wrapped = wrapped;

        public override void OnCompleted() => _wrapped.OnCompleted();

        public override void OnError(Exception error) => _wrapped.OnError(error);

        protected override void Handle(T value) => _wrapped.OnNext(value);
    }
}
