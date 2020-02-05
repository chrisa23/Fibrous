using System;
using System.Threading.Tasks;

namespace Fibrous.Agents
{
    /// <summary>
    ///     Actor like abstraction.  Receives a single type of message through a channel
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AsyncChannelAgent<T> : IDisposable
    {
        protected readonly IAsyncFiber Fiber;

        public AsyncChannelAgent(IChannel<T> channel, Func<T, Task> handler, Action<Exception> errorCallback)
        {
            Fiber = new AsyncFiber(errorCallback);
            channel.Subscribe(Fiber, handler);
        }

        public void Dispose()
        {
            Fiber.Dispose();
        }
    }
}