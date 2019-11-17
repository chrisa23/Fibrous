using System;
using System.Threading.Tasks;

namespace Fibrous
{
    /// <summary>
    ///     Default executor that simply
    /// </summary>
    public sealed class AsyncExecutor : IAsyncExecutor
    {
        public async Task Execute(Func<Task> toExecute)
        {
            await toExecute();
        }
    }
}