using System;

namespace Fibrous
{
    public class StubFiberFactory : IFiberFactory
    {
        private readonly IFiberScheduler _scheduler;
        private readonly IAsyncFiberScheduler _asyncScheduler;

        public StubFiberFactory(IFiberScheduler scheduler = null,
            IAsyncFiberScheduler asyncScheduler = null)
        {
            _scheduler = scheduler;
            _asyncScheduler = asyncScheduler;
        }
        public IFiber Create()
        {
            return new StubFiber(scheduler: _scheduler);
        }

        public IFiber Create(Action<Exception> errorHandler)
        {
            return new StubFiber(errorHandler, _scheduler);
        }

        public IAsyncFiber CreateAsync(Action<Exception> errorHandler)
        {
            return new AsyncStubFiber(errorHandler, _asyncScheduler);
        }
    }
}