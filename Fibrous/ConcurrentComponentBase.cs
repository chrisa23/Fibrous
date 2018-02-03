namespace Fibrous
{
    using System;

    public abstract class ConcurrentComponentBase : IDisposable, IScheduler
    {
        protected IFiber Fiber;

        protected ConcurrentComponentBase(IExecutor executor = null, FiberType type = FiberType.Pool)
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

        public void Dispose()
        {
            Fiber?.Dispose();
        }
    }
}