using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Fibrous;

/// <summary>
///     Default executor that simply awaits running the async method
/// </summary>
public sealed class AsyncExecutor : IAsyncExecutor
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task ExecuteAsync(Func<Task> toExecute) => toExecute();
}
