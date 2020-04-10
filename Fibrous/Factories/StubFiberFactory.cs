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


        public IFiber Create(Action<Exception> errorHandler = null)
        {
            return errorHandler == null
                ? new StubFiber(scheduler: _scheduler)
                : new StubFiber(errorHandler, _scheduler);
        }

        public IAsyncFiber CreateAsync(Action<Exception> errorHandler)
        {
            return new AsyncStubFiber(errorHandler, _asyncScheduler);
        }
    }
}