namespace Fibrous
{
    using System;

    /// <summary>
    /// Fiber that executes on caller's thread.  For testing and well understood situations.  Use with caution.
    /// </summary>
    public sealed class StubFiber : FiberBase
    {
        public StubFiber(Executor excecutor, IFiberScheduler scheduler) : base(excecutor, scheduler)
        {
        }

        public StubFiber(Executor executor) : base(executor)
        {
        }

        public StubFiber()
        {
        }

        protected override void InternalEnqueue(Action action)
        {
            Executor.Execute(action);
        }

        public static IFiber StartNew()
        {
            var fiber = new StubFiber();
            return fiber.Start();
        }

        public static IFiber StartNew(Executor executor)
        {
            var fiber = new StubFiber(executor);
            return fiber.Start();
        }
    }
}