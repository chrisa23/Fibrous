using System;

namespace Fibrous.Fibers
{
    public sealed class StubFiber : FiberBase
    {
        public override void Enqueue(Action action)
        {
            action();
        }

        public override void Start()
        {
        }
    }
}