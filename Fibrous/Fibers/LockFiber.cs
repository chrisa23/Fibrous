using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Fibrous;

public class LockFiber : FiberBase
{
    private readonly Action _flushCache;
    private readonly object _lock = new();
    private readonly ArrayQueue<Action> _queue;
    private readonly TaskFactory _taskFactory;
    private bool _flushPending;

    public LockFiber(IExecutor executor = null, int size = QueueSize.DefaultQueueSize,
        TaskFactory taskFactory = null, IFiberScheduler scheduler = null)
        : base(executor, scheduler)
    {
        _queue = new ArrayQueue<Action>(size);
        _taskFactory = taskFactory ??
                       new TaskFactory(TaskCreationOptions.PreferFairness, TaskContinuationOptions.None);
        _flushCache = Flush;
    }

    public LockFiber(Action<Exception> errorCallback, int size = QueueSize.DefaultQueueSize,
        TaskFactory taskFactory = null, IFiberScheduler scheduler = null)
        : this(new ExceptionHandlingExecutor(errorCallback), size, taskFactory, scheduler)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void InternalEnqueue(Action action)
    {
        AggressiveSpinWait spinWait = default;
        while (_queue.IsFull)
        {
            spinWait.SpinOnce();
        }

        lock (_lock)
        {
            _queue.Enqueue(action);

            if (_flushPending)
            {
                return;
            }

            _flushPending = true;
            _ = _taskFactory.StartNew(_flushCache);
        }
    }

    private void Flush()
    {
        (int count, Action[] actions) = Drain();

        for (int i = 0; i < count; i++)
        {
            Executor.Execute(actions[i]);
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
    private (int, Action[]) Drain()
    {
        lock (_lock)
        {
            return _queue.Drain();
        }
    }
}
