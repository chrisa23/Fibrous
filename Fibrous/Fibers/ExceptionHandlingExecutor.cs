using System;
using System.Threading.Tasks;

namespace Fibrous;

/// <summary>
///     Executor that catches exceptions and forwards them to an optional callback.
/// </summary>
public sealed class ExceptionHandlingExecutor(Action<Exception> callback = null, IExecutor inner = null) : IExecutor
{
    public async Task ExecuteAsync(Func<Task> toExecute)
    {
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
        catch (Exception exception)
        {
            callback?.Invoke(exception);
        }
    }
}
