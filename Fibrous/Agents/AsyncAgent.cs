using System;
using System.Threading.Tasks;

namespace Fibrous.Agents
{
    /// <summary>
    ///     Agent using injected handler function.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AsyncAgent<T> : IAgent<T>
    {
        private readonly IPublisherPort<T> _channel;
        protected readonly IAsyncFiber Fiber;

        public AsyncAgent(Func<T, Task> handler, IAsyncExecutor executor = null)
        {
            Fiber = AsyncFiber.StartNew(executor);
            _channel = Fiber.NewChannel(handler);
        }

        public void Publish(T msg)
        {
            _channel.Publish(msg);
        }

        public void Dispose()
        {
            Fiber.Dispose();
        }
    }
}