namespace Fibrous.Fibers
{
    using System;

    public sealed class StubFiber : FiberBase
    {
        public StubFiber(FiberConfig config) : base(config)
        {
        }

        public StubFiber()
            : this(FiberConfig.Default)
        {
        }

        public override void Enqueue(Action action)
        {
            Executor.Execute(action);
        }

        public override void Start()
        {
        }
    }
}