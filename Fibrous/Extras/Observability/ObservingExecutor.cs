using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Fibrous.Extras.Observability;

/// <summary>
///     Executor wrapper that records execution duration and success or failure.
/// </summary>
public sealed class ObservingExecutor : IExecutor
{
    private readonly IExecutor _inner;
    private readonly Action<ExecutionObservation> _observe;

    public ObservingExecutor(
        Action<ExecutionObservation> observe,
        IExecutor inner = null)
    {
        _observe = observe ?? throw new ArgumentNullException(nameof(observe));
        _inner = inner ?? new Executor();
    }

    public async Task ExecuteAsync(Func<Task> toExecute)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        Exception exception = null;

        try
        {
            await _inner.ExecuteAsync(toExecute);
        }
        catch (Exception ex)
        {
            exception = ex;
            throw;
        }
        finally
        {
            _observe(new ExecutionObservation(stopwatch.Elapsed, exception));
        }
    }
}
