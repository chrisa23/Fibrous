using System;
using System.Threading.Tasks;

namespace Fibrous;

/// <summary>
///     Executor that catches exceptions and forwards them to an optional callback.
/// </summary>
public sealed class ExceptionHandlingExecutor : IExecutor
{
    private readonly Action<Exception> _callback;
    private readonly IExecutor _inner;

    public ExceptionHandlingExecutor(Action<Exception> callback = null, IExecutor inner = null)
    {
        _callback = callback;
        _inner = inner ?? new Executor();
    }

    public async Task ExecuteAsync(Func<Task> toExecute)
    {
        try
        {
            await _inner.ExecuteAsync(toExecute);
        }
        catch (Exception exception)
        {
            _callback?.Invoke(exception);
        }
    }
}
