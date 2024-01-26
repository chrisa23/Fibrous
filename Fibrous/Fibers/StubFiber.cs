using System;
using System.Threading.Tasks;

namespace Fibrous;

/// <summary>
///     Fiber that executes on caller's thread.  For testing and well understood situations.  Use with caution.
/// </summary>
public sealed class StubFiber : FiberBase
{
    public StubFiber(IExecutor executor = null, IAsyncFiberScheduler scheduler = null)
        : base(executor, scheduler)
    {
    }

    public StubFiber(Action<Exception> errorCallback, IAsyncFiberScheduler scheduler = null)
        : this(new ExceptionHandlingExecutor(errorCallback), scheduler)
    {
    }

#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
    protected override void InternalEnqueue(Func<Task> action) => Executor.ExecuteAsync(action).Wait();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
}
