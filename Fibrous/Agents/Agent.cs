using System;

namespace Fibrous.Agents
{
    /// <summary>
    ///     Agent using injected handler function.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Agent<T> :  IAgent<T>
    {
        private readonly Action<T> _handler;
        protected IFiber Fiber;
        public Agent(Action<T> handler, Action<Exception> errorCallback = null)
        {
            _handler = handler;
            Fiber = errorCallback == null? new Fiber() : new Fiber(errorCallback);
        }

        public Agent(IFiberFactory factory, Action<T> handler, Action<Exception> errorCallback = null)
        {
            _handler = handler;
            Fiber = errorCallback == null ? factory.Create() : factory.Create(errorCallback);
        }

        public void Publish(T msg)
        {
            Fiber.Enqueue(() => _handler( msg));
        }

        public void Dispose()
        {
            Fiber.Dispose();
        }
    }

 
}