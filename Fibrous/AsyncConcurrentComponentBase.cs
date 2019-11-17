using System;
using System.Threading.Tasks;

namespace Fibrous
{
    public abstract class AsyncConcurrentComponentBase : IDisposable, IAsyncScheduler
    {
        protected IAsyncFiber Fiber;

        protected AsyncConcurrentComponentBase(IAsyncExecutor executor = null)
        {
            Fiber = AsyncFiber.StartNew(executor);
        }

        public void Dispose()
        {
            Fiber?.Dispose();
        }

        public IDisposable Schedule(Func<Task> action, TimeSpan dueTime)
        {
            return Fiber.Schedule(action, dueTime);
        }

        public IDisposable Schedule(Func<Task> action, TimeSpan startTime, TimeSpan interval)
        {
            return Fiber.Schedule(action, startTime, interval);
        }
    }
}