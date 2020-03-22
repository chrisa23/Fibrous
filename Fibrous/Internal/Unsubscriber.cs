using System;

namespace Fibrous
{
    internal sealed class Unsubscriber : IDisposable
    {
        private readonly IDisposable _disposable;
        private readonly IDisposableRegistry _disposables;
        private readonly SingleShotGuard _guard = new SingleShotGuard();

        public Unsubscriber(IDisposable disposable, IDisposableRegistry disposables)
        {
            _disposable = disposable;
            _disposables = disposables;
            disposables.Add(_disposable);
        }

        public void Dispose()
        {
            if (!_guard.Check) return;

            _disposables.Remove(_disposable);
            _disposable.Dispose();
        }
    }
}