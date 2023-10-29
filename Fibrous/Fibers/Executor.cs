using System;
using System.Runtime.CompilerServices;

namespace Fibrous;

/// <summary>
///     Default executor that simply executes the action
/// </summary>
public sealed class Executor : IExecutor
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Execute(Action toExecute) => toExecute();
}
