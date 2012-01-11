using System;

namespace Fibrous
{
    public interface IRequestPort<in TRequest, out TReply>
    {
        TReply SendRequest(TRequest request, TimeSpan timeout);
    }
}