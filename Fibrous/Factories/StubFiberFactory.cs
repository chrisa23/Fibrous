using System;

namespace Fibrous
{
    public class StubFiberFactory : IFiberFactory
    {
        private readonly IAsyncFiberScheduler _asyncScheduler;
        private readonly IFiberScheduler _scheduler;

        public StubFiberFactory(IFiberScheduler scheduler = null,
            IAsyncFiberScheduler asyncScheduler = null)
        {
            _scheduler = scheduler;
            _asyncScheduler = asyncScheduler;
        }


        public IFiber CreateFiber(Action<Exception> errorHandler = null) =>
            errorHandler == null
                ? new StubFiber(scheduler: _scheduler)
                : new StubFiber(errorHandler, _scheduler);

        public IAsyncFiber CreateAsyncFiber(Action<Exception> errorHandler) =>
            new AsyncStubFiber(errorHandler, _asyncScheduler);
    }
}
