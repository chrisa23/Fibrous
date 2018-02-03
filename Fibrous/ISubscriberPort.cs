namespace Fibrous
{
    using System;

    /// <summary>
    /// Port for subscribing to messages
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISubscriberPort<out T>
    {
        /// <summary>
        /// Subscribe to messages on this channel with a fiber and handler.
        /// </summary>
        /// <param name="fiber"></param>
        /// <param name="receive"></param>
        /// <returns></returns>
        IDisposable Subscribe(IFiber fiber, Action<T> receive);
    }
}