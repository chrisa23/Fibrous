using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Fibrous;

/// <summary>
///     It is suggested to always use an Exception callback with the IAsyncFiber
/// </summary>
public class Fiber : FiberBase
{
    private const TaskCreationOptions FlushTaskCreationOptions = TaskCreationOptions.DenyChildAttach;
    private readonly Func<Task> _flushCache;
    private readonly object _lock = new();
    private readonly ArrayQueue<Func<Task>> _queue;
    private readonly TaskScheduler _taskScheduler;
    private bool _flushPending;

    public Fiber(IExecutor executor = null, int size = QueueSize.DefaultQueueSize,
        IFiberScheduler scheduler = null)
        : base(executor, scheduler)
    {
        _queue = new ArrayQueue<Func<Task>>(size);
        _taskScheduler = TaskScheduler.Default;
        _flushCache = FlushAsync;
    }

    public Fiber(Action<Exception> errorCallback, int size = QueueSize.DefaultQueueSize,
        IFiberScheduler scheduler = null)
        : this(new ExceptionHandlingExecutor(errorCallback), size, scheduler)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void InternalEnqueue(Func<Task> action)
    {
        AggressiveSpinWait spinWait = default;
        while (true)
        {
            lock (_lock)
            {
                if (_queue.IsFull)
                {
                    goto Spin;
                }

                _queue.Enqueue(action);

                if (_flushPending)
                {
                    return;
                }

                _flushPending = true;
                ScheduleFlush();
                return;
            }

Spin:
            spinWait.SpinOnce();
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
                ScheduleFlush();
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

    private void ScheduleFlush() =>
        _ = Task.Factory.StartNew(_flushCache,
                CancellationToken.None,
                FlushTaskCreationOptions,
                _taskScheduler)
            .Unwrap();
}
