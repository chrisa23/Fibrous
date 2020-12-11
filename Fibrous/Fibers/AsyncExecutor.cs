using System;
using System.Threading.Tasks;

namespace Fibrous
{
    /// <summary>
    ///     Default executor that simply awaits running the async method
    /// </summary>
    public sealed class AsyncExecutor : IAsyncExecutor
    {
        public Task ExecuteAsync(Func<Task> toExecute) => toExecute();
    }
}
