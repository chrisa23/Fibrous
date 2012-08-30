namespace Fibrous.Utility
{
    using System;
    using System.Collections.Generic;

    public class Disposables : IDisposableRegistry
    {
        private readonly object _lock = new object();
        private readonly List<IDisposable> _items = new List<IDisposable>();
        private bool _disposed;

        public void Add(IDisposable toAdd)
        {
            lock (_lock)
            {
                _items.Add(toAdd);
            }
        }

        public void Remove(IDisposable toRemove)
        {
            lock (_lock)
            {
                _items.Remove(toRemove);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    DisposeOfMembers();
                }
                _disposed = true;
            }
        }

        private void DisposeOfMembers()
        {
            IDisposable[] disposables;
            lock (_lock)
            {
                disposables = _items.ToArray();
                _items.Clear();
            }
            foreach (IDisposable victim in disposables)
            {
                victim.Dispose();
            }
        }
    }
}