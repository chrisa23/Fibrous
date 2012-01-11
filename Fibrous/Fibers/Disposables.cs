using System;
using System.Collections.Generic;

namespace Fibrous.Fibers
{
    public class Disposables : IDisposableRegistry
    {
        private readonly object _lock = new object();
        private readonly List<IDisposable> _items = new List<IDisposable>();

        public void Add(IDisposable toAdd)
        {
            lock (_lock)
            {
                _items.Add(toAdd);
            }
        }

        public bool Remove(IDisposable toRemove)
        {
            lock (_lock)
            {
                return _items.Remove(toRemove);
            }
        }

        public virtual void Dispose()
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

        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _items.Count;
                }
            }
        }
    }
}