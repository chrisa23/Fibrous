using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Fibrous;

public abstract class AsyncFiberBase : IAsyncFiber
{
    private readonly Disposables _disposables = new();
    private readonly IAsyncFiberScheduler _fiberScheduler;
    protected readonly IAsyncExecutor Executor;
    private bool _disposed;

    protected AsyncFiberBase(IAsyncExecutor executor = null, IAsyncFiberScheduler scheduler = null)
    {
        _fiberScheduler = scheduler ?? new AsyncTimerScheduler();
        Executor = executor ?? new AsyncExecutor();
    }

    public IDisposable Schedule(Func<Task> action, TimeSpan dueTime) =>
        _fiberScheduler.Schedule(this, action, dueTime);

    public IDisposable Schedule(Func<Task> action, TimeSpan startTime, TimeSpan interval) =>
        _fiberScheduler.Schedule(this, action, startTime, interval);

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
