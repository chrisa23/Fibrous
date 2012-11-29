namespace Fibrous.Fibers
{
    using System;

    public sealed class StubFiber : FiberBase
    {
        public StubFiber(IExecutor executor) : base(executor)
        {
        }

        public StubFiber()
        {
        }

        public override void Enqueue(Action action)
        {
            Executor.Execute(action);
        }

        public override IFiber Start()
        {
            return this;
        }
    }
}