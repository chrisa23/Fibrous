using System;

namespace Fibrous.Utility
{
    public sealed class DisposeAction : IDisposable
    {
        private readonly Action _action;

        public DisposeAction(Action action)
        {
            _action = action;
        }

        #region IDisposable Members

        public void Dispose()
        {
            _action();
        }

        #endregion
    }
}