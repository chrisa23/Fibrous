using System;
using System.Threading.Tasks;

namespace Fibrous;

/// <summary>
///     IExecutor that handles any exceptions thrown with an optional exception callback
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
        catch (Exception e)
        {
            _callback?.Invoke(e);
        }
    }
}
