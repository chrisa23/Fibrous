using System;
using System.Windows.Threading;

namespace Fibrous.WPF;

public class WpfFiberFactory(
    Dispatcher dispatcher = null,
    DispatcherPriority priority = DispatcherPriority.Normal)
    : IFiberFactory
{
    public IFiber CreateAsyncFiber(Action<Exception> errorHandler) =>
        new DispatcherFiber(errorHandler, dispatcher, priority);
}
