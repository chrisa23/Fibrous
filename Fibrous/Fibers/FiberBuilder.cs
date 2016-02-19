namespace Fibrous
{
    using System;
    using System.Threading;
    using Fibrous.Queues;

    public interface IFiberBuilder
    {
        IFiberBuilder WithName(string name);
        IFiberBuilder WithPriority(ThreadPriority priority);
        IFiberBuilder WithExecutor(IExecutor executor);
        IFiberBuilder WithErrorHandlingExecutor(Action<Exception> callback = null);
        IFiberBuilder WithQueue(IQueue queue);
        IFiberBuilder WithSleepingQueue();
        IFiberBuilder WithYieldingQueue();
        IFiberBuilder WithBlockingQueue(int maxItems);
        IFiber Build();
        IFiber Start();
    }

    public class FiberBuilderImpl : IFiberBuilder
    {
        private readonly FiberType _type;
        private string _name;
        private ThreadPriority _priority = ThreadPriority.Normal;
        private IQueue _queue;
        private IFiberScheduler _scheduler;
        private IExecutor _executor;

        public FiberBuilderImpl(FiberType type)
        {
            _type = type;
        }

        public IFiberBuilder WithName(string name)
        {
            if (_type != FiberType.Thread)
                throw new ArgumentException("Can only name a Thread Fiber");
            _name = name;
            return this;
        }

        public IFiberBuilder WithPriority(ThreadPriority priority)
        {
            if (_type != FiberType.Thread)
                throw new ArgumentException("Can only set priority for a Thread Fiber");
            _priority = priority;
            return this;
        }

        public IFiberBuilder WithExecutor(IExecutor executor)
        {
            _executor = executor;
            return this;
        }

        public IFiberBuilder WithErrorHandlingExecutor(Action<Exception> callback = null)
        {
            _executor = new ExceptionHandlingExecutor(callback);
            return this;
        }

        public IFiberBuilder WithQueue(IQueue queue)
        {
            if (_type != FiberType.Thread)
                throw new ArgumentException("Can only set queue for a Thread Fiber");
            _queue = queue;
            return this;
        }

        public IFiberBuilder WithSleepingQueue()
        {
            if (_type != FiberType.Thread)
                throw new ArgumentException("Can only set queue for a Thread Fiber");
            _queue = new SleepingQueue();
            return this;
        }

        public IFiberBuilder WithYieldingQueue()
        {
            if (_type != FiberType.Thread)
                throw new ArgumentException("Can only set queue for a Thread Fiber");
            _queue = new YieldingQueue();
            return this;
        }

        public IFiberBuilder WithBlockingQueue(int maxItems)
        {
            if (_type != FiberType.Thread)
                throw new ArgumentException("Can only set queue for a Thread Fiber");
            _queue = new BoundedQueue(maxItems);
            return this;
        }

        public IFiber Build()
        {
            if (_executor == null) _executor = new Executor();
            IFiber fiber;
            switch (_type)
            {
                case FiberType.Thread:
                    fiber = new ThreadFiber(_executor, new TimerScheduler(), _queue, _name, true, _priority);
                    break;
                case FiberType.Pool:
                    fiber = new PoolFiber(_executor);
                    break;
                case FiberType.Stub:
                    fiber = new StubFiber(_executor);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return fiber;
        }

        public IFiber Start()
        {
            IFiber fiber = Build();
            fiber.Start();
            return fiber;
        }
    }

    /// <summary>
    /// Fluent builder for Thread Fiber creation
    /// </summary>
    public static class FiberBuilder
    {
        public static IFiberBuilder Create(FiberType type)
        {
            return new FiberBuilderImpl(type);
        }
    }
}