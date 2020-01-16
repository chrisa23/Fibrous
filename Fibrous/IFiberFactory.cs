using System;
using System.Collections.Generic;
using System.Text;

namespace Fibrous
{
    public interface IFiberFactory
    {
        IFiber Create();
        IFiber Create(Action<Exception> errorHandler);
        IAsyncFiber CreateAsync(Action<Exception> errorHandler);
    }

    public class DefaultFiberFactory : IFiberFactory
    {
        public IFiber Create()
        {
            return new Fiber();
        }

        public IFiber Create(Action<Exception> errorHandler)
        {
            return new Fiber(errorHandler);
        }

        public IAsyncFiber CreateAsync(Action<Exception> errorHandler)
        {
            return new AsyncFiber(errorHandler);
        }
    }

    public class StubFiberFactory : IFiberFactory
    {
        public IFiber Create()
        {
            return new StubFiber();
        }

        public IFiber Create(Action<Exception> errorHandler)
        {
            return new StubFiber(errorHandler);
        }

        public IAsyncFiber CreateAsync(Action<Exception> errorHandler)
        {
            return new AsyncStubFiber(errorHandler);
        }
    }
}
