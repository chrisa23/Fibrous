using System;

namespace Fibrous.Utility
{
    internal sealed class Unsubscriber : IDisposable
    {
        private readonly IDisposable _disposable;
        private readonly IDisposableRegistry _disposables;

        public Unsubscriber(IDisposable disposable, IDisposableRegistry disposables)
        {
            _disposable = disposable;
            _disposables = disposables;
            disposables.Add(_disposable);
        }

        #region IDisposable Members

        public void Dispose()
        {
            _disposables.Remove(_disposable);
            _disposable.Dispose();
        }

        #endregion
    }
}