namespace Fibrous
{
    using System;

    public interface IRequestPort<in TRequest, out TReply>
    {
        TReply SendRequest(TRequest request, TimeSpan timeout);
    }
}