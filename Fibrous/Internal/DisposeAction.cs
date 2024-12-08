using System;

namespace Fibrous;

internal sealed class DisposeAction(Action action) : IDisposable
{
    private readonly SingleShotGuard _guard = new();

    public void Dispose()
    {
        if (_guard.Check)
        {
            action();
        }
    }
}
