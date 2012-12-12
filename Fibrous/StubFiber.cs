namespace Fibrous
{
    using System;

    public sealed class StubFiber : Fiber
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

        public static Fiber StartNew()
        {
            var fiber = new StubFiber();
            return fiber.Start();
        }

        public static Fiber StartNew(Executor executor)
        {
            var fiber = new StubFiber(executor);
            return fiber.Start();
        }
    }
}