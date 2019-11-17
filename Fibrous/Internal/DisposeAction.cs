using System;

namespace Fibrous
{
    internal sealed class DisposeAction : IDisposable
    {
        private readonly Action _action;
        private readonly SingleShotGuard _guard = new SingleShotGuard();

        public DisposeAction(Action action)
        {
            _action = action;
        }

        public void Dispose()
        {
            if (_guard.Check)
                _action();
        }
    }
}