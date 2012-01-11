using System;
using Fibrous.Internal;

namespace Fibrous.Fibers
{
    public abstract class FiberBase : Disposables, IFiber
    {
        public abstract void Start();
        public abstract void Enqueue(Action action);
    }
}