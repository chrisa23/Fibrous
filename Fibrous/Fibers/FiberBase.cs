using System;
using Fibrous.Utility;

namespace Fibrous.Fibers
{
    public abstract class FiberBase : Disposables, IFiber
    {
        #region IFiber Members

        public abstract void Start();
        public abstract void Enqueue(Action action);

        #endregion
    }
}