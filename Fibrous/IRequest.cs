namespace Fibrous
{
    /// <summary>
    /// Interface for requests where a handler can send a reply
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TReply"></typeparam>
    public interface IRequest<out TRequest, in TReply>
    {
        /// <summary>
        /// The request 
        /// </summary>
        TRequest Request { get; }

        /// <summary>
        /// Reply to the request 
        /// </summary>
        /// <param name="reply"></param>
        void Reply(TReply reply);
    }
}