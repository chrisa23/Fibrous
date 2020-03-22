using System;
using System.Collections.Generic;
using System.Text;

namespace Fibrous
{
    public abstract class FiberComponent : IHaveFiber
    {
        public IFiber Fiber { get; }

        protected FiberComponent()
        {
            Fiber = new Fiber(OnError);
        }
        protected FiberComponent(IFiberFactory factory)
        {
            Fiber = factory.Create(OnError);
        }

        protected abstract void OnError(Exception obj);

        public void Dispose()
        {
            Fiber?.Dispose();
        }
    }
}
