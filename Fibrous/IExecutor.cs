using System;
using System.Threading.Tasks;

namespace Fibrous;

/// <summary>
///     Abstraction of handling execution.  Allows insertion of exception handling, profiling,
///     etc.
/// </summary>
public interface IExecutor
{
    Task ExecuteAsync(Func<Task> toExecute);
}
