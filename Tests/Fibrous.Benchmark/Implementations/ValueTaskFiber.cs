using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Fibrous.Benchmark.Implementations
{
    public interface IValueAsyncFiber : IAsyncScheduler, IDisposableRegistry
    {
        void Enqueue(Func<ValueTask> action);

        //Rest of API
        //void Enqueue(Func<Task> action);
        //IDisposable Schedule(Func<Task> action, TimeSpan dueTime);
        //IDisposable Schedule(Func<Task> action, TimeSpan startTime, TimeSpan interval);
        //IDisposable Schedule(Func<Task> action, DateTime when);
        //IDisposable Schedule(Func<Task> action, DateTime when, TimeSpan interval);
        //IDisposable CronSchedule(Func<Task> action, string cron);
        //IDisposable Subscribe<T>(ISubscriberPort<T> channel, Func<T, Task> handler);
        //IDisposable SubscribeToBatch<T>(ISubscriberPort<T> port, Func<T[], Task> receive, TimeSpan interval);
        //IDisposable SubscribeToKeyedBatch<TKey, T>(ISubscriberPort<T> port, Converter<T, TKey> keyResolver, Func<IDictionary<TKey, T>, Task> receive, TimeSpan interval);
        //IDisposable SubscribeToLast<T>(ISubscriberPort<T> port, Func<T, Task> receive, TimeSpan interval);
        //IDisposable Subscribe<T>(ISubscriberPort<T> port, Func<T, Task> receive, Predicate<T> filter);
        //IChannel<T> NewChannel<T>(Func<T, Task> onEvent);
        //IRequestPort<TRq, TRp> NewRequestPort<TRq, TRp>(Func<IRequest<TRq, TRp>, Task> onEvent);
    }

    public interface IAsyncScheduler
    {
        /// <summary>
        ///     Schedule an action to be executed once
        /// </summary>
        /// <param name="action"></param>
        /// <param name="dueTime"></param>
        /// <returns></returns>
        IDisposable Schedule(Func<ValueTask> action, TimeSpan dueTime);

        /// <summary>
        ///     Schedule an action to be taken repeatedly
        /// </summary>
        /// <param name="action"></param>
        /// <param name="startTime"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        IDisposable Schedule(Func<ValueTask> action, TimeSpan startTime, TimeSpan interval);
    }


    /// <summary>
    ///     It is suggested to always use an Exception callback with the IAsyncFiber
    /// </summary>
    public class ValueAsyncFiber : ValueAsyncFiberBase
    {
        private readonly Func<ValueTask> _flushCache;
        private readonly ArrayQueue<Func<ValueTask>> _queue;
        private readonly TaskFactory _taskFactory;
        private bool _flushPending;
        private SpinLock _spinLock = new SpinLock(false);

        public ValueAsyncFiber(IValueAsyncExecutor executor = null, int size = QueueSize.DefaultQueueSize,
            TaskFactory taskFactory = null, IValueAsyncFiberScheduler scheduler = null)
            : base(executor, scheduler)
        {
            _queue = new ArrayQueue<Func<ValueTask>>(size);
            _taskFactory = taskFactory ??
                           new TaskFactory(TaskCreationOptions.PreferFairness, TaskContinuationOptions.None);
            _flushCache = Flush;
        }

        public ValueAsyncFiber(Action<Exception> errorCallback, int size = QueueSize.DefaultQueueSize,
            TaskFactory taskFactory = null, IValueAsyncFiberScheduler scheduler = null)
            : this(new ValueAsyncExceptionHandlingExecutor(errorCallback), size, taskFactory, scheduler)
        {
        }

        protected override void InternalEnqueue(Func<ValueTask> action)
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
                _taskFactory.StartNew(_flushCache);
            }
            finally
            {
                if (lockTaken)
                {
                    _spinLock.Exit(false);
                }
            }
        }

        private async ValueTask Flush()
        {
            (int count, Func<ValueTask>[] actions) = Drain();

            for (int i = 0; i < count; i++)
            {
                await Executor.Execute(actions[i]);
            }

            bool lockTaken = false;
            try
            {
                _spinLock.Enter(ref lockTaken);

                if (_queue.Count > 0)
                    //don't monopolize thread.
#pragma warning disable 4014
                {
                    _taskFactory.StartNew(_flushCache);
                }
#pragma warning restore 4014
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
        private (int, Func<ValueTask>[]) Drain()
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


    public abstract class ValueAsyncFiberBase : Disposables, IValueAsyncFiber
    {
        private readonly IValueAsyncFiberScheduler _fiberScheduler;
        protected readonly IValueAsyncExecutor Executor;
        private bool _disposed;

        protected ValueAsyncFiberBase(IValueAsyncExecutor executor = null, IValueAsyncFiberScheduler scheduler = null)
        {
            _fiberScheduler = scheduler ?? new ValueAsyncTimerScheduler();
            Executor = executor ?? new ValueAsyncExecutor();
        }

        public IDisposable Schedule(Func<ValueTask> action, TimeSpan dueTime) =>
            _fiberScheduler.Schedule(this, action, dueTime);

        public IDisposable Schedule(Func<ValueTask> action, TimeSpan startTime, TimeSpan interval) =>
            _fiberScheduler.Schedule(this, action, startTime, interval);

        public void Enqueue(Func<ValueTask> action)
        {
            if (_disposed)
            {
                return;
            }

            InternalEnqueue(action);
        }

        public override void Dispose()
        {
            _disposed = true;
            base.Dispose();
        }

        protected abstract void InternalEnqueue(Func<ValueTask> action);
    }

    public interface IValueAsyncExecutor
    {
        ValueTask Execute(Func<ValueTask> toExecute);
    }

    public sealed class ValueAsyncExecutor : IValueAsyncExecutor
    {
        public async ValueTask Execute(Func<ValueTask> toExecute) => await toExecute();
    }


    internal sealed class ValueAsyncTimerScheduler : IValueAsyncFiberScheduler
    {
        public IDisposable Schedule(IValueAsyncFiber fiber, Func<ValueTask> action, TimeSpan dueTime)
        {
            if (dueTime.TotalMilliseconds <= 0)
            {
                ValueAsyncPendingAction pending = new ValueAsyncPendingAction(action);
                fiber.Enqueue(pending.Execute);
                return pending;
            }

            return new ValueAsyncTimerAction(fiber, action, dueTime);
        }

        public IDisposable
            Schedule(IValueAsyncFiber fiber, Func<ValueTask> action, TimeSpan dueTime, TimeSpan interval) =>
            new ValueAsyncTimerAction(fiber, action, dueTime, interval);
    }

    public interface IValueAsyncFiberScheduler
    {
        IDisposable Schedule(IValueAsyncFiber fiber, Func<ValueTask> action, TimeSpan dueTime);
        IDisposable Schedule(IValueAsyncFiber fiber, Func<ValueTask> action, TimeSpan startTime, TimeSpan interval);
    }

    internal sealed class ValueAsyncPendingAction : IDisposable
    {
        private readonly Func<ValueTask> _action;
        private bool _cancelled;

        public ValueAsyncPendingAction(Func<ValueTask> action) => _action = action;

        public void Dispose() => _cancelled = true;

        public ValueTask Execute()
        {
            if (_cancelled)
            {
                return new ValueTask();
            }

            return _action();
        }
    }

    internal sealed class ValueAsyncTimerAction : IDisposable
    {
        private readonly Func<ValueTask> _action;
        private readonly TimeSpan _interval;
        private bool _cancelled;
        private Timer _timer;

        public ValueAsyncTimerAction(IValueAsyncFiber fiber, Func<ValueTask> action, TimeSpan dueTime)
        {
            _action = action;
            _interval = TimeSpan.FromMilliseconds(-1);
            _timer = new Timer(x => ExecuteOnTimerThread(fiber), null, dueTime, _interval);
            fiber.Add(this);
        }

        public ValueAsyncTimerAction(IValueAsyncFiber fiber, Func<ValueTask> action, TimeSpan dueTime,
            TimeSpan interval)
        {
            _action = action;
            _interval = interval;
            _timer = new Timer(x => ExecuteOnTimerThread(fiber), null, dueTime, interval);
            fiber.Add(this);
        }

        public void Dispose()
        {
            _cancelled = true;
            DisposeTimer();
        }

        private void ExecuteOnTimerThread(IValueAsyncFiber fiber)
        {
            if (_interval.Ticks == TimeSpan.FromMilliseconds(-1).Ticks || _cancelled)
            {
                fiber.Remove(this);
                DisposeTimer();
            }

            if (!_cancelled)
            {
                fiber.Enqueue(Execute);
            }
        }

        private ValueTask Execute()
        {
            if (_cancelled)
            {
                return new ValueTask();
            }

            return _action();
        }

        private void DisposeTimer()
        {
            _timer?.Dispose();
            _timer = null;
        }
    }

    public sealed class ValueAsyncExceptionHandlingExecutor : IValueAsyncExecutor
    {
        private readonly Action<Exception> _callback;

        public ValueAsyncExceptionHandlingExecutor(Action<Exception> callback = null) => _callback = callback;

        public async ValueTask Execute(Func<ValueTask> toExecute)
        {
            try
            {
                await toExecute();
            }
            catch (Exception e)
            {
                _callback?.Invoke(e);
            }
        }
    }
}
