namespace Fibrous
{
    using System;

    public interface IRequestPort<in TRequest, out TReply>
    {
        //PostAndReply
        TReply SendRequest(TRequest request, TimeSpan timeout);
    }
}