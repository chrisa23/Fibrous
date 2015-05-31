namespace Fibrous
{
    using System;

    /// <summary>
    /// Simple singleton point of event publishing by type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class EventBus<T> 
    {
        public static readonly IChannel<T> Channel = new Channel<T>();

        public static IDisposable Subscribe(IFiber fiber, Action<T> receive)
        {
            return Channel.Subscribe(fiber, receive);
        }

        public static bool Publish(T msg)
        {
            return Channel.Publish(msg);
        }
    }
}