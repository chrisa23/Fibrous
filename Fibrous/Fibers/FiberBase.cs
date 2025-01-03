// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Fibrous;

public abstract class FiberBase(IExecutor executor = null, IAsyncFiberScheduler scheduler = null)
    : IFiber
{
    private readonly   Disposables          _disposables = new();
    private readonly   IAsyncFiberScheduler _fiberScheduler = scheduler ?? new AsyncTimerScheduler();
    protected readonly IExecutor            Executor = executor ?? new Executor();
    private            bool                 _disposed;

    public IDisposable Schedule(Func<Task> action, TimeSpan dueTime) =>
        _fiberScheduler.Schedule(this, action, dueTime);

    public IDisposable Schedule(Func<Task> action, TimeSpan startTime, TimeSpan interval) =>
        _fiberScheduler.Schedule(this, action, startTime, interval);

    public IDisposable Schedule(Action action, TimeSpan dueTime) =>
        Schedule(action.ToAsync(), dueTime);

    public IDisposable Schedule(Action action, TimeSpan startTime, TimeSpan interval) =>
        Schedule(action.ToAsync(), startTime, interval);

    public void Enqueue(Action action)
    {
        if (_disposed)
        {
            return;
        }

        InternalEnqueue(action.ToAsync());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Enqueue(Func<Task> action)
    {
        if (_disposed)
        {
            return;
        }

        InternalEnqueue(action);
    }

    public void Dispose()
    {
        _disposed = true;
        _disposables.Dispose();
    }

    public void Add(IDisposable toAdd) => _disposables.Add(toAdd);

    public void Remove(IDisposable toRemove) => _disposables.Remove(toRemove);

    protected abstract void InternalEnqueue(Func<Task> action);
}
