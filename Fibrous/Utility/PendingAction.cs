using System;

namespace Fibrous.Utility
{
    public sealed class PendingAction : IDisposable
    {
        private readonly Action _action;
        private bool _cancelled;

        public PendingAction(Action action)
        {
            _action = action;
        }

        #region IDisposable Members

        public void Dispose()
        {
            _cancelled = true;
        }

        #endregion

        public void Execute()
        {
            if (!_cancelled)
                _action();
        }
    }
}