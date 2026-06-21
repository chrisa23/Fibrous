using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Fibrous.Extras.Observability;

/// <summary>
///     Executor wrapper that records execution duration and success or failure.
/// </summary>
public sealed class ObservingExecutor(
    Action<ExecutionObservation> observe,
    IExecutor                    inner = null)
    : IExecutor
{
    private readonly Action<ExecutionObservation> _observe = observe ?? throw new ArgumentNullException(nameof(observe));

    public async Task ExecuteAsync(Func<Task> toExecute)
    {
        long start = Stopwatch.GetTimestamp();
        Exception exception = null;

        try
        {
            if (inner is null)
            {
                await toExecute();
            }
            else
            {
                await inner.ExecuteAsync(toExecute);
            }
        }
        catch (Exception ex)
        {
            exception = ex;
            throw;
        }
        finally
        {
            _observe(new ExecutionObservation(GetElapsed(start), exception));
        }
    }

    private static TimeSpan GetElapsed(long startTimestamp)
    {
#if NET7_0_OR_GREATER
        return Stopwatch.GetElapsedTime(startTimestamp);
#else
        long elapsedTicks = (long)((Stopwatch.GetTimestamp() - startTimestamp)
                                   * (TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency));
        return new TimeSpan(elapsedTicks);
#endif
    }
}
