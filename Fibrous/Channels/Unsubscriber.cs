namespace Fibrous.Channels
{
    using System;

    internal sealed class Unsubscriber<T> : IDisposable
    {
        private readonly IDisposable _disposable;
        private readonly IDisposableRegistry _disposables;

        public Unsubscriber(IDisposable disposable, IDisposableRegistry disposables)
        {
            _disposable = disposable;
            _disposables = disposables;
            disposables.Add(_disposable);
        }

        public void Dispose()
        {
            _disposables.Remove(_disposable);
            _disposable.Dispose();
        }
    }
}