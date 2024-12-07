using System;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Fibrous.WPF;

/// <summary>
///     Fiber for use with WPF forms and controls.Provides seamless marshalling to dispatcher thread.
/// </summary>
public sealed class DispatcherFiber(
    IExecutor executor = null,
    Dispatcher dispatcher = null,
    DispatcherPriority priority = DispatcherPriority.Normal)
    : FiberBase(executor)
{
    private readonly Dispatcher _dispatcher = dispatcher ?? Dispatcher.CurrentDispatcher;

    public DispatcherFiber(Action<Exception> errorCallback, Dispatcher dispatcher = null,
        DispatcherPriority priority = DispatcherPriority.Normal)
        : this(new ExceptionHandlingExecutor(errorCallback), dispatcher, priority)
    {
    }

    protected override void InternalEnqueue(Func<Task> action) => _dispatcher.InvokeAsync(action, priority);
}
