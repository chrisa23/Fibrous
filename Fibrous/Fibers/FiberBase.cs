namespace Fibrous.Fibers
{
    using System;
    using Fibrous.Utility;

    public abstract class FiberBase : Disposables, IFiber
    {
        public abstract void Start();
        public abstract void Enqueue(Action action);
    }
}