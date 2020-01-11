using System;
using System.Threading.Tasks;

namespace Fibrous
{
    /// <summary>
    ///     IExecutor that handles any exceptions thrown with an optional exception callback
    /// </summary>
    public sealed class AsyncExceptionHandlingExecutor : IAsyncExecutor
    {
        private readonly Action<Exception> _callback;

        public AsyncExceptionHandlingExecutor(Action<Exception> callback = null)
        {
            _callback = callback;
        }
        
        public async Task Execute(Func<Task> toExecute)
        {
            try
            {
                await toExecute();
            }
            catch (Exception e)
            {
                _callback?.Invoke(e);
            }
        }
    }
}