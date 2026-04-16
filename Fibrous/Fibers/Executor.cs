using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Fibrous;

/// <summary>
///     Default executor that directly invokes the provided asynchronous delegate.
/// </summary>
public sealed class Executor : IExecutor
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task ExecuteAsync(Func<Task> toExecute) => toExecute();
}
