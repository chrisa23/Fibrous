using System;
using System.Threading.Tasks;

namespace Fibrous;

internal sealed class PendingAction(Func<Task> action) : IDisposable
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
