using System;
using System.Collections.Generic;

namespace Fibrous
{
    /// <summary>
    ///     Collection of disposables, where they can be removed or Disposed together.
    ///     Mostly for internal use, but very convenient for grouping and handling disposables
    /// </summary>
    public interface IDisposableRegistry : IDisposable
    {
        /// <summary>
        ///     Add an IDisposable to the registry.  It will be disposed when the registry is disposed.
        /// </summary>
        /// <param name="toAdd"></param>
        void Add(IDisposable toAdd);

        /// <summary>
        ///     Remove a disposable from the registry.  It will not be disposed when the registry is disposed.
        /// </summary>
        /// <param name="toRemove"></param>
        void Remove(IDisposable toRemove);
    }

    public class Disposables : IDisposableRegistry
    {
        private readonly SingleShotGuard _guard;
        private readonly List<IDisposable> _items = new List<IDisposable>();
        private readonly object _lock = new object();

        public Disposables()
        {
        }

        public Disposables(IEnumerable<IDisposable> initial) => _items.AddRange(initial);

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

            foreach (IDisposable victim in disposables)
            {
                victim.Dispose();
            }
        }
    }
}
