using System;

namespace Fibrous.Extras.Observability;

/// <summary>
///     Observation emitted after a fiber work item finishes executing.
/// </summary>
public readonly struct ExecutionObservation
{
    public ExecutionObservation(TimeSpan elapsed, Exception exception)
    {
        Elapsed = elapsed;
        Exception = exception;
    }

    public TimeSpan Elapsed   { get; }
    public Exception Exception { get; }
    public bool      Succeeded => Exception is null;
}
