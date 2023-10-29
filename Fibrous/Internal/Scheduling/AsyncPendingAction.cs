using System;
using System.Threading.Tasks;

namespace Fibrous;

internal sealed class AsyncPendingAction : IDisposable
{
    private readonly Func<Task> _action;
    private bool _cancelled;

    public AsyncPendingAction(Func<Task> action) => _action = action;

    public void Dispose() => _cancelled = true;

    public Task ExecuteAsync()
    {
        if (_cancelled)
        {
            return Task.CompletedTask;
        }

        return _action();
    }
}
