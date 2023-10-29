using System;
using System.Windows.Threading;

namespace Fibrous.WPF;

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

    public IFiber CreateFiber(Action<Exception> errorHandler = null) =>
        errorHandler == null
            ? new DispatcherFiber(dispatcher: _dispatcher, priority: _priority)
            : new DispatcherFiber(errorHandler, _dispatcher, _priority);

    public IAsyncFiber CreateAsyncFiber(Action<Exception> errorHandler) =>
        new AsyncDispatcherFiber(errorHandler, _dispatcher, _priority);
}
