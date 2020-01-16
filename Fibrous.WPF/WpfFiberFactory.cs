using System;

namespace Fibrous.WPF
{
    public class WpfFiberFactory : IFiberFactory
    {
        public IFiber Create()
        {
            return new DispatcherFiber();
        }

        public IFiber Create(Action<Exception> errorHandler)
        {
            return new DispatcherFiber(errorHandler);
        }

        public IAsyncFiber CreateAsync(Action<Exception> errorHandler)
        {
            return new AsyncDispatcherFiber(errorHandler);
        }
    }
}