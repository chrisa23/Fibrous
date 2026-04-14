using System;
using System.Threading.Tasks;

namespace Fibrous;

/// <summary>
///     Executor that catches exceptions and forwards them to an optional callback.
/// </summary>
public sealed class ExceptionHandlingExecutor : IExecutor
{
    private readonly Action<Exception> _callback;

    public ExceptionHandlingExecutor(Action<Exception> callback = null) => _callback = callback;

    public async Task ExecuteAsync(Func<Task> toExecute)
    {
        try
        {
            await toExecute();
        }
        catch (Exception exception)
        {
            _callback?.Invoke(exception);
        }
    }
}
