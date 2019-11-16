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

        public async Task Execute(int count, Func<Task>[] actions)
        {
            for (var i = 0; i < count; i++)
            {
                var action = actions[i];
                await Execute(action);
            }
        }
    }
}