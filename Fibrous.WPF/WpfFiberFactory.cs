using System;
using System.Windows.Threading;

namespace Fibrous.WPF
{
    public class WpfFiberFactory : IFiberFactory
    {
        private readonly Dispatcher _dispatcher;
        private readonly DispatcherPriority _priority;

        public WpfFiberFactory(Dispatcher dispatcher = null,
            DispatcherPriority priority = DispatcherPriority.Normal)
        {
            _dispatcher = dispatcher;
            _priority = priority;
        }
        public IFiber Create()
        {
            return new DispatcherFiber(dispatcher: _dispatcher, priority: _priority);
        }

        public IFiber Create(Action<Exception> errorHandler)
        {
            return new DispatcherFiber(errorHandler, _dispatcher, _priority);
        }

        public IAsyncFiber CreateAsync(Action<Exception> errorHandler)
        {
            return new AsyncDispatcherFiber(errorHandler, _dispatcher, _priority);
        }
    }
}