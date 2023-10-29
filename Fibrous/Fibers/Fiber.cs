using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Fibrous;

public class Fiber : FiberBase
{
    private readonly Action _flushCache;
    private readonly ArrayQueue<Action> _queue;
    private readonly TaskFactory _taskFactory;
    private bool _flushPending;
    private SpinLock _spinLock = new(false);

    public Fiber(IExecutor executor = null, int size = QueueSize.DefaultQueueSize, TaskFactory taskFactory = null,
        IFiberScheduler scheduler = null)
        : base(executor, scheduler)
    {
        _queue = new ArrayQueue<Action>(size);
        _taskFactory = taskFactory ??
                       new TaskFactory(TaskCreationOptions.PreferFairness, TaskContinuationOptions.None);
        _flushCache = Flush;
    }

    public Fiber(Action<Exception> errorCallback, int size = QueueSize.DefaultQueueSize,
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

        bool lockTaken = false;
        try
        {
            _spinLock.Enter(ref lockTaken);

            _queue.Enqueue(action);

            if (_flushPending)
            {
                return;
            }

            _flushPending = true;
            _ = _taskFactory.StartNew(_flushCache);
        }
        finally
        {
            if (lockTaken)
            {
                _spinLock.Exit(false);
            }
        }
    }

    private void Flush()
    {
        (int count, Action[] actions) = Drain();

        for (int i = 0; i < count; i++)
        {
            Action execute = actions[i];
            Executor.Execute(execute);
        }

        bool lockTaken = false;
        try
        {
            _spinLock.Enter(ref lockTaken);

            if (_queue.Count > 0)
            {
                _ = _taskFactory.StartNew(_flushCache);
            }
            else
            {
                _flushPending = false;
            }
        }
        finally
        {
            if (lockTaken)
            {
                _spinLock.Exit(false);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private (int, Action[]) Drain()
    {
        bool lockTaken = false;
        try
        {
            _spinLock.Enter(ref lockTaken);

            return _queue.Drain();
        }
        finally
        {
            if (lockTaken)
            {
                _spinLock.Exit(false);
            }
        }
    }
}

public class Fiber2 : FiberBase
{
    private readonly WaitCallback _flushCache;
    private readonly ArrayQueue<Action> _queue;
    private readonly TaskFactory _taskFactory;
    private bool _flushPending;
    private SpinLock _spinLock = new(false);

    public Fiber2(IExecutor executor = null, int size = QueueSize.DefaultQueueSize, TaskFactory taskFactory = null,
        IFiberScheduler scheduler = null)
        : base(executor, scheduler)
    {
        _queue = new ArrayQueue<Action>(size);
        _taskFactory = taskFactory ??
                       new TaskFactory(TaskCreationOptions.PreferFairness, TaskContinuationOptions.None);
        _flushCache = Flush;
    }

    public Fiber2(Action<Exception> errorCallback, int size = QueueSize.DefaultQueueSize,
        TaskFactory taskFactory = null, IFiberScheduler scheduler = null)
        : this(new ExceptionHandlingExecutor(errorCallback), size, taskFactory, scheduler)
    {
    }

    protected override void InternalEnqueue(Action action)
    {
        AggressiveSpinWait spinWait = default;
        while (_queue.IsFull)
        {
            spinWait.SpinOnce();
        }

        bool lockTaken = false;
        try
        {
            _spinLock.Enter(ref lockTaken);

            _queue.Enqueue(action);

            if (_flushPending)
            {
                return;
            }

            _flushPending = true;
            ThreadPool.QueueUserWorkItem(Flush, null);
        }
        finally
        {
            if (lockTaken)
            {
                _spinLock.Exit(false);
            }
        }
    }

    private void Flush(object o)
    {
        (int count, Action[] actions) = Drain();

        for (int i = 0; i < count; i++)
        {
            Action execute = actions[i];
            Executor.Execute(execute);
        }

        bool lockTaken = false;
        try
        {
            _spinLock.Enter(ref lockTaken);

            if (_queue.Count > 0)
            {
                //don't monopolize thread.
                ThreadPool.QueueUserWorkItem(Flush, null);
            }
            else
            {
                _flushPending = false;
            }
        }
        finally
        {
            if (lockTaken)
            {
                _spinLock.Exit(false);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private (int, Action[]) Drain()
    {
        bool lockTaken = false;
        try
        {
            _spinLock.Enter(ref lockTaken);

            return _queue.Drain();
        }
        finally
        {
            if (lockTaken)
            {
                _spinLock.Exit(false);
            }
        }
    }
}
