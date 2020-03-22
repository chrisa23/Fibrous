using System;
using System.Collections.Generic;
using System.Text;

namespace Fibrous
{
    public abstract class ConcurrentComponent : IHaveFiber
    {
        public IFiber Fiber { get; }

        protected ConcurrentComponent()
        {
            Fiber = new Fiber(OnError);
        }
        protected ConcurrentComponent(IFiberFactory factory)
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
