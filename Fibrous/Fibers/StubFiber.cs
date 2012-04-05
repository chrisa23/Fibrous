namespace Fibrous.Fibers
{
    using System;

    public sealed class StubFiber : FiberBase
    {
        private readonly IExecutor _executor;

        public StubFiber(IExecutor executor)
        {
            _executor = executor;
        }

        public StubFiber()
            : this(new DefaultExecutor())
        {
        }

        public override void Enqueue(Action action)
        {
            _executor.Execute(action);
        }

        public override void Start()
        {
        }
    }
}