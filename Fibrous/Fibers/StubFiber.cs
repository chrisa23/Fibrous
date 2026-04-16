using System;
using System.Threading.Tasks;

namespace Fibrous;

/// <summary>
///     Fiber that executes on caller's thread.  For testing and well understood situations.  Use with caution.
/// </summary>
public sealed class StubFiber(IExecutor executor = null, IFiberScheduler scheduler = null)
    : FiberBase(executor, scheduler)
{
    public StubFiber(Action<Exception> errorCallback, IFiberScheduler scheduler = null)
        : this(new ExceptionHandlingExecutor(errorCallback), scheduler)
    {
    }

#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
    protected override void InternalEnqueue(Func<Task> action) => Executor.ExecuteAsync(action).Wait();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
}
