using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Fibrous;

/// <summary>
///     It is suggested to always use an Exception callback with the IAsyncFiber
/// </summary>
public class Fiber : FiberBase
{
    private readonly Func<Task> _flushCache;
    private readonly object _lock = new();
    private readonly ArrayQueue<Func<Task>> _queue;
    private readonly TaskFactory _taskFactory;
    private bool _flushPending;

    public Fiber(IExecutor executor = null, int size = QueueSize.DefaultQueueSize,
        TaskFactory taskFactory = null, IFiberScheduler scheduler = null)
        : base(executor, scheduler)
    {
        _queue = new ArrayQueue<Func<Task>>(size);
        _taskFactory = taskFactory ??
                       new TaskFactory(TaskCreationOptions.PreferFairness, TaskContinuationOptions.None);
        _flushCache = FlushAsync;
    }

    public Fiber(Action<Exception> errorCallback, int size = QueueSize.DefaultQueueSize,
        TaskFactory taskFactory = null, IFiberScheduler scheduler = null)
        : this(new ExceptionHandlingExecutor(errorCallback), size, taskFactory, scheduler)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void InternalEnqueue(Func<Task> action)
    {
        AggressiveSpinWait spinWait = default;
        //SpinWait spinWait = new SpinWait();
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

    private async Task FlushAsync()
    {
        (int count, Func<Task>[] actions) = Drain();

        for (int i = 0; i < count; i++)
        {
            await Executor.ExecuteAsync(actions[i]);
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
    private (int, Func<Task>[]) Drain()
    {
        lock (_lock)
        {
            return _queue.Drain();
        }
    }
}
