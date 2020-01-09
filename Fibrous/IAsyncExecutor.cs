using System;
using System.Threading.Tasks;

namespace Fibrous
{   
    /// <summary>
    ///     Abstraction of handling execution.  Allows insertion of exception handling, profiling,
    ///     etc.
    /// </summary>
    public interface IAsyncExecutor
    {
        Task Execute(Func<Task> toExecute);
    }
}