namespace Fibrous
{
    using System;

    public interface ISubscriberPort<out T>
    {
        /// <summary>
        /// Subscribe 
        /// </summary>
        /// <param name="fiber"></param>
        /// <param name="receive"></param>
        /// <returns></returns>
        IDisposable Subscribe(Fiber fiber, Action<T> receive);
    }
}