using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fibrous
{
    //for some reason EventHub is about 4x slower to publish through.......????
    public sealed class EventHub : IEventHub
    {
        //concurrent dict and no lock?
        private readonly ConcurrentDictionary<Type, object> _channels = new ConcurrentDictionary<Type, object>();

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
        
        //20 ns for this with no subscribers (now 16ns with changes)
        public void Publish<T>(T msg)
        {
            var type = msg.GetType(); 
            
            if (!_channels.ContainsKey(type)) return;

            IChannel<T> channel = (IChannel<T>) _channels[type];
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
            var channel = (IChannel<T>) _channels.GetOrAdd(type, _ => new Channel<T>());
            return channel.Subscribe(fiber, receive.Handle);
        }

        // ReSharper disable once UnusedMember.Local
        private IDisposable AsyncSubscribeToChannel<T>(IAsyncFiber fiber, IHandleAsync<T> receive)
        {
            var type = typeof(T);
            var channel = (IChannel<T>)_channels.GetOrAdd(type, _ => new Channel<T>());
            return channel.Subscribe(fiber, receive.Handle);
        }

        internal bool HasSubscriptions<T>()
        {
            var type = typeof(T);

            if (!_channels.ContainsKey(type)) return false;

           Channel<T> channel = (Channel<T>)_channels[type];
           return channel.HasSubscriptions;
        }
    }
}