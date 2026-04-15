using System;
using System.Collections.Generic;

namespace Fibrous;

/// <summary>
///     Registry of disposables that can be removed individually or disposed together.
/// </summary>
public interface IDisposableRegistry : IDisposable
{
    /// <summary>
    ///     Adds a disposable to the registry. It will be disposed when the registry is disposed.
    /// </summary>
    /// <param name="toAdd">Disposable to add.</param>
    void Add(IDisposable toAdd);

    /// <summary>
    ///     Removes a disposable from the registry. It will not be disposed with the registry.
    /// </summary>
    /// <param name="toRemove">Disposable to remove.</param>
    void Remove(IDisposable toRemove);
}

public class Disposables : IDisposableRegistry
{
    private readonly SingleShotGuard _guard = new();
    private readonly List<IDisposable> _items = new();
    private readonly object _lock = new();
    private bool _disposed;

    public Disposables()
    {
    }

    public Disposables(IEnumerable<IDisposable> initial) => _items.AddRange(initial);

    public void Add(IDisposable toAdd)
    {
        bool disposeImmediately = false;
        lock (_lock)
        {
            if (_disposed)
            {
                disposeImmediately = true;
            }
            else
            {
                _items.Add(toAdd);
            }
        }

        if (disposeImmediately)
        {
            toAdd.Dispose();
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
            if (_disposed)
            {
                return;
            }

            disposables = _items.ToArray();
            _items.Clear();
            _disposed = true;
        }

        foreach (IDisposable victim in disposables)
        {
            victim.Dispose();
        }
    }
}
