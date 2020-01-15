using System;
using System.Threading.Tasks;

namespace Fibrous
{
    /// <summary>
    ///     Port for sending requests and receiving replies.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TReply"></typeparam>
    public interface IRequestPort<in TRequest, TReply>
    {
        /// <summary>
        ///     Send an asynchronous request, and let the reply be delivered to the fiber when ready
        /// </summary>
        /// <param name="request"></param>
        /// <param name="fiber"></param>
        /// <param name="onReply"></param>
        /// <returns></returns>
        IDisposable SendRequest(TRequest request, IFiber fiber, Action<TReply> onReply);

        /// <summary>
        ///     Send an asynchronous request, and let the reply be delivered to the fiber when ready
        /// </summary>
        /// <param name="request"></param>
        /// <param name="fiber"></param>
        /// <param name="onReply"></param>
        /// <returns></returns>
        IDisposable SendRequest(TRequest request, IAsyncFiber fiber, Func<TReply, Task> onReply);

        /// <summary>
        ///     Send an asynchronous request and get a reply object for handling the response in the same code block.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<TReply> SendRequest(TRequest request);

        Task<Result<TReply>> SendRequest(TRequest request, TimeSpan timeout);
    }

    public class Result<T>
    {
        public T Value { get; set; }
        public bool Succeeded { get; set; }
        public static Result<T> Ok(T value) => new Result<T> {Succeeded = true, Value = value};
        public static Result<T> Failed {get;} = new Result<T> { Succeeded = false };
    }
}