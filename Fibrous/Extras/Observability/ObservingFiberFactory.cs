using System;

namespace Fibrous.Extras.Observability;

/// <summary>
///     Factory that creates fibers whose work is observed before optional exception handling swallows failures.
/// </summary>
public sealed class ObservingFiberFactory(
    Action<ExecutionObservation> observe,
    int size = QueueSize.DefaultQueueSize,
    IFiberScheduler scheduler = null)
    : IFiberFactory
{
    public IFiber CreateFiber(Action<Exception> errorHandler)
    {
        IExecutor observed = new ObservingExecutor(observe);
        IExecutor handled = new ExceptionHandlingExecutor(errorHandler, observed);

        return new Fiber(handled, size, scheduler);
    }
}
