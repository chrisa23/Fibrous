using System;
using System.Threading.Tasks;

namespace Fibrous;

/// <summary>
///     Abstraction for executing work, allowing exception handling or instrumentation to be layered in.
/// </summary>
public interface IExecutor
{
    Task ExecuteAsync(Func<Task> toExecute);
}
