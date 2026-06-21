using System;

namespace Fibrous.Extras.Observability;

/// <summary>
///     Factory that creates fibers whose work is observed before optional exception handling swallows failures.
/// </summary>
public sealed class ObservingFiberFactory(
    Action<ExecutionObservation> observe,
    int                          size      = QueueSize.DefaultQueueSize,
    IFiberScheduler              scheduler = null)
    : IFiberFactory
{
    private readonly Action<ExecutionObservation> _observe = observe ?? throw new ArgumentNullException(nameof(observe));

    public IFiber CreateFiber(Action<Exception> errorHandler) =>
        new Fiber(
            new ExceptionHandlingExecutor(errorHandler, new ObservingExecutor(_observe)),
            size,
            scheduler);
}
