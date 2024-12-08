using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Fibrous.Extras;

public abstract class AsyncObserver<T> : IObserver<T>
{
    private readonly Action _flushCache;
    private readonly object _lock = new();
    private readonly ArrayQueue<T> _queue;
    private readonly TaskFactory _taskFactory;
    private bool _completed;
    private Exception _error;
    private bool _errored;
    private bool _flushPending;
    private bool _stopped;

    protected AsyncObserver(int size = QueueSize.DefaultQueueSize, TaskFactory taskFactory = null)
    {
        _queue = new ArrayQueue<T>(size);
        _taskFactory = taskFactory ??
                       new TaskFactory(TaskCreationOptions.PreferFairness, TaskContinuationOptions.None);
        _flushCache = Flush;
    }

    public void OnCompleted()
    {
        lock (_lock)
        {
            _completed = true;
            if (!_flushPending)
            {
                _stopped = true;
                HandleCompleted();
            }
        }
    }

    public void OnError(Exception error)
    {
        lock (_lock)
        {
            _errored = true;

            if (!_flushPending)
            {
                _stopped = true;
                HandleError(error);
            }
            else
            {
                _error = error;
            }
        }
    }

    public void OnNext(T item)
    {
        AggressiveSpinWait spinWait = default;
        while (_queue.IsFull)
        {
            spinWait.SpinOnce();
        }

        lock (_lock)
        {
            if (_stopped)
            {
                return;
            }

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
    protected abstract void HandleCompleted();
    protected abstract void HandleError(Exception exception);

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

                if (_errored)
                {
                    _stopped = true;
                    HandleError(_error);
                    return;
                }

                if (_completed)
                {
                    _stopped = true;
                    HandleCompleted();
                }
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

public sealed class AsyncObserverWrapper<T>(IObserver<T> wrapped) : AsyncObserver<T>
{
    protected override void HandleCompleted() => wrapped.OnCompleted();

    protected override void HandleError(Exception error) => wrapped.OnError(error);

    protected override void Handle(T value) => wrapped.OnNext(value);
}
