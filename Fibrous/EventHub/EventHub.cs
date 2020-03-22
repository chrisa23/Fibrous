using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fibrous
{
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
        // ReSharper disable once UnusedMember.Local
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

        // ReSharper disable once UnusedMember.Local
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