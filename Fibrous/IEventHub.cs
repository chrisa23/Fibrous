using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Fibrous
{

    /// <summary>
    ///   Injectable event hub where classes implementing a combination of
    ///   IHaveFiber / IHandle or IHaveAsyncFiber / IHandleAsync
    ///   can auto wire up to receive events publish on the 
    /// </summary>
    public interface IEventHub
    {
        IDisposable Subscribe(object handler);
        void Publish<T>(T msg);
    }

    /// <summary>
    ///     For use with IEventHub to auto wire events
    ///     Denotes a class which has a regular Fiber.
    ///     Must be used in conjunction with IHandle<T>
    /// </summary>
    public interface IHaveFiber
    {
        IFiber Fiber { get; }
    }

    /// <summary>
    ///     For use with IEventHub to auto wire events
    ///     Denotes a class which can handle a particular type of message.
    ///     Must be used in conjunction with IHaveFiber
    /// </summary>
    /// <typeparam name="TMessage">The type of message to handle.</typeparam>
    // ReSharper disable once TypeParameterCanBeVariant
    public interface IHandle<TMessage>
    {
        void Handle(TMessage message);
    }

    /// <summary>
    ///     For use with IEventHub to auto wire events
    ///     Denotes a class which has an async Fiber.
    ///     Must be used in conjunction with IHandleAsync<T>
    /// </summary>
    public interface IHaveAsyncFiber
    {
        IAsyncFiber Fiber { get; }
    }
    
    /// <summary>
    ///     For use with IEventHub to auto wire events
    ///     Denotes a class which can handle a particular type of message.
    ///     Must be used in conjunction with IHaveAsyncFiber
    /// </summary>
    /// <typeparam name="TMessage">The type of message to handle.</typeparam>
    // ReSharper disable once TypeParameterCanBeVariant
    public interface IHandleAsync<TMessage>
    {
        Task Handle(TMessage message);
    }
    
    public sealed class EventHub : IEventHub
    {
        private readonly Dictionary<Type, object> _channels = new Dictionary<Type, object>();

        public IDisposable Subscribe(object handler)
        {
            bool regularFiber = handler is IHaveFiber;
            bool asyncFiber = handler is IHaveAsyncFiber;
            if (!(regularFiber || asyncFiber) || (regularFiber && asyncFiber))
                throw new ArgumentException("Handler must implement one of the interfaces, IHaveFiber or IHaveAsyncFiber");

            object fiber = regularFiber ? (object)((IHaveFiber)handler).Fiber : ((IHaveAsyncFiber)handler).Fiber;

            var disposable = SetupHandlers(handler, fiber, regularFiber);

            return new Unsubscriber(disposable, (IDisposableRegistry) fiber);
        }
        
        public void Publish<T>(T msg)
        {
            var type = typeof(T);
            IChannel<T> channel;
            lock (_channels)
            {
                if (!_channels.ContainsKey(type)) return;

                channel = (IChannel<T>) _channels[type];
            }
            channel.Publish(msg);
        }

        private IDisposable SetupHandlers(object handler, object fiber, bool regular)
        {
            var interfaceType = regular ? typeof(IHandle<>) : typeof(IHandleAsync<>);
            var subMethod = regular ? "SubscribeToChannel" : "AsyncSubscribeToChannel";
            var interfaces = handler.GetType().GetTypeInfo().ImplementedInterfaces.Where(x =>
                x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == interfaceType);

            var disposables = new Disposables();

            foreach (var @interface in interfaces)
            {
                var type = @interface.GetTypeInfo().GenericTypeArguments[0];
                var method = @interface.GetRuntimeMethod("Handle", new[] { type });

                if (method == null) continue;

                var sub = GetType().GetTypeInfo().GetDeclaredMethod(subMethod).MakeGenericMethod(type);

                var dispose = sub.Invoke(this, new[] { fiber, handler }) as IDisposable;
                disposables.Add(dispose);
            }

            return disposables;
        }
        private IDisposable SubscribeToChannel<T>(IFiber fiber, IHandle<T> receive)
        {
            var type = typeof(T);
            IChannel<T> channel;
            lock (_channels)
            {
                if (!_channels.ContainsKey(type))
                    _channels.Add(type, new Channel<T>());

                channel = ((IChannel<T>) _channels[type]);
            }
            return channel.Subscribe(fiber, receive.Handle);
        }

        private IDisposable AsyncSubscribeToChannel<T>(IAsyncFiber fiber, IHandleAsync<T> receive)
        {
            var type = typeof(T);
            IChannel<T> channel;
            lock (_channels)
            {
                if (!_channels.ContainsKey(type))
                    _channels.Add(type, new Channel<T>());

                channel = (IChannel<T>) _channels[type];
            }
            return channel.Subscribe(fiber, receive.Handle);
        }
    }
}