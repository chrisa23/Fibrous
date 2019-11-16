using System;

namespace Fibrous
{
    internal sealed class DisposeAction : IDisposable
    {
        private readonly Action _action;
        private bool _disposed;

        public DisposeAction(Action action)
        {
            _action = action;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _action();
        }
    }
}