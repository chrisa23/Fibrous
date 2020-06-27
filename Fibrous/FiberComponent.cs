using System;
using System.Collections.Generic;
using System.Text;

namespace Fibrous
{
    public abstract class FiberComponent : IDisposable
    {
        protected IFiber Fiber { get; }

        protected FiberComponent(IFiberFactory factory = null)
        {
            Fiber = factory?.Create(OnError) ?? new Fiber(OnError);
        }

        protected abstract void OnError(Exception obj);

        public void Dispose()
        {
            Fiber?.Dispose();
        }
    }
}
