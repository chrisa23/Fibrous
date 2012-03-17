namespace Fibrous.Fibers
{
    using System;

    public abstract class FiberBase : Disposables, IFiber
    {
        public abstract void Start();
        public abstract void Enqueue(Action action);
    }
}