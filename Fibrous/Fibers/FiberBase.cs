using System;
using System.Runtime.CompilerServices;

namespace Fibrous;

public abstract class FiberBase : IFiber
{
    private readonly Disposables _disposables = new();
    private readonly IFiberScheduler _fiberScheduler;
    protected readonly IExecutor Executor;
    private volatile bool _disposed;

    protected FiberBase(IExecutor executor = null, IFiberScheduler scheduler = null)
    {
        _fiberScheduler = scheduler ?? new TimerScheduler();
        Executor = executor ?? new Executor();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Enqueue(Action action)
    {
        if (_disposed)
        {
            return;
        }

        InternalEnqueue(action);
    }

    public IDisposable Schedule(Action action, TimeSpan dueTime) => _fiberScheduler.Schedule(this, action, dueTime);

    public IDisposable Schedule(Action action, TimeSpan startTime, TimeSpan interval) =>
        _fiberScheduler.Schedule(this, action, startTime, interval);

    public void Dispose()
    {
        _disposed = true;
        _disposables.Dispose();
    }

    public void Add(IDisposable toAdd) => _disposables.Add(toAdd);

    public void Remove(IDisposable toRemove) => _disposables.Remove(toRemove);

    protected abstract void InternalEnqueue(Action action);
}
