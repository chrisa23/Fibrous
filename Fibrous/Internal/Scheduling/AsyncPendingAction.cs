using System;
using System.Threading.Tasks;

namespace Fibrous;

internal sealed class AsyncPendingAction(Func<Task> action) : IDisposable
{
    private bool _cancelled;

    public void Dispose() => _cancelled = true;

    public Task ExecuteAsync()
    {
        if (_cancelled)
        {
            return Task.CompletedTask;
        }

        return action();
    }
}
