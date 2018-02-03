namespace Fibrous
{
    using System;
    using Fibrous.Util;

    public abstract class ConcurrentComponant : Disposables, IScheduler
    {
        protected IFiber Fiber;

        protected ConcurrentComponant(IExecutor executor = null, FiberType type = FiberType.Pool)
        {
            Fiber = Fibrous.Fiber.StartNew(type, executor);
        }

        public IDisposable Schedule(Action action, TimeSpan dueTime)
        {
            return Fiber.Schedule(action, dueTime);
        }

        public IDisposable Schedule(Action action, TimeSpan startTime, TimeSpan interval)
        {
            return Fiber.Schedule(action, startTime, interval);
        }
    }
}