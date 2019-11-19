using System;
using System.Collections.Generic;

namespace Fibrous
{
    public class Disposables : IDisposableRegistry
    {
        private readonly SingleShotGuard _guard = new SingleShotGuard();
        private readonly List<IDisposable> _items = new List<IDisposable>();
        private readonly object _lock = new object();
        
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

        public virtual void Dispose()
        {
            if (_guard.Check)
            {
                DisposeOfMembers();
                GC.SuppressFinalize(this);
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

            foreach (var victim in disposables)
                victim.Dispose();
        }
    }
}