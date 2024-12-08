using System;

namespace Fibrous;

internal sealed class PendingAction(Action action) : IDisposable
{
    private bool _cancelled;

    public void Dispose() => _cancelled = true;

    public void Execute()
    {
        if (!_cancelled)
        {
            action();
        }
    }
}
