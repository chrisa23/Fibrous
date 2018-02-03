namespace Fibrous
{
    using System;

    /// <summary>
    /// Future type for receiving a response
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IReply<out T>
    {
        /// <summary>
        /// Call to wait for a reply to be delivereed.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        IResult<T> Receive(TimeSpan timeout);
    }
}