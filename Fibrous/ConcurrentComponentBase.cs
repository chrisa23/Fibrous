using System;

namespace Fibrous
{
    public abstract class ConcurrentComponentBase : IDisposable, IScheduler
    {
        protected IFiber Fiber;

        protected ConcurrentComponentBase(IExecutor executor = null)
        {
            Fiber = PoolFiber.StartNew(executor);
        }

        public void Dispose()
        {
            Fiber?.Dispose();
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