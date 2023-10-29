using System;

namespace Fibrous;

/// <summary>
///     IExecutor that handles any exceptions thrown with an optional exception callback
/// </summary>
public sealed class ExceptionHandlingExecutor : IExecutor
{
    private readonly Action<Exception> _callback;

    public ExceptionHandlingExecutor(Action<Exception> callback = null) => _callback = callback;

    public void Execute(Action toExecute)
    {
        try
        {
            toExecute();
        }
        catch (Exception e)
        {
            _callback?.Invoke(e);
        }
    }
}
