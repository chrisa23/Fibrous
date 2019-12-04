using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Fibrous
{
    /// <summary>
    ///     Denotes a class which can handle a particular type of message.
    /// </summary>
    /// <typeparam name="TMessage">The type of message to handle.</typeparam>
    // ReSharper disable once TypeParameterCanBeVariant
    public interface IHandle<TMessage>
    {
        /// <summary>
        ///     Handles the message.
        /// </summary>

        IFiber Fiber { get; }

        void Handle(TMessage message);
    }

    // ReSharper disable once TypeParameterCanBeVariant
    public interface IHandleAsync<TMessage>
    {
        /// <summary>
        ///     Handles the message.
        /// </summary>

        IAsyncFiber Fiber { get; }

        Task Handle(TMessage message);
    }

    public interface IEventAggregator
    {
        IDisposable Subscribe(object handler);
        void Publish<T>(T msg);
    }

    public class EventAggregator : IEventAggregator
    {
        private static readonly Type[] Types = {typeof(IHandle<>), typeof(IHandleAsync<>)};
        private readonly Dictionary<Type, object> _channels = new Dictionary<Type, object>();

        public IDisposable Subscribe(object handler)
        {
            var interfaces = handler.GetType().GetTypeInfo().ImplementedInterfaces
                .Where(x => x.GetTypeInfo().IsGenericType && Types.Contains(x.GetGenericTypeDefinition()));
            var disposable = new Disposables();
            foreach (var @interface in interfaces)
            {
                var type = @interface.GetTypeInfo().GenericTypeArguments[0];
                var method = @interface.GetRuntimeMethod("Handle", new[] {type});

                if (method != null)
                {
                    var ourSubscribe = @interface.GetGenericTypeDefinition() == typeof(IHandle<>)
                        ? "SubscribeToChannel"
                        : "AsyncSubscribeToChannel";

                    var sub = GetType().GetTypeInfo().GetDeclaredMethod(ourSubscribe).MakeGenericMethod(type);

                    var dispose = sub.Invoke(this, new[] {handler}) as IDisposable;
                    disposable.Add(dispose);
                }
            }

            return disposable;
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

        private IDisposable SubscribeToChannel<T>(IHandle<T> receive)
        {
            var type = typeof(T);
            IChannel<T> channel;
            lock (_channels)
            {
                if (!_channels.ContainsKey(type))
                    _channels.Add(type, new Channel<T>());

                channel = ((IChannel<T>) _channels[type]);
            }
            return channel.Subscribe(receive.Fiber, receive.Handle);
        }

        private IDisposable AsyncSubscribeToChannel<T>(IHandleAsync<T> receive)
        {
            var type = typeof(T);
            IChannel<T> channel;
            lock (_channels)
            {
                if (!_channels.ContainsKey(type))
                    _channels.Add(type, new Channel<T>());

                channel = (IChannel<T>) _channels[type];
            }
            return channel.Subscribe(receive.Fiber, receive.Handle);
        }
    }
}