namespace Fibrous
{
    using System;

    public interface IReply<T>
    {
        Result<T> Receive(TimeSpan timeout);
    }
}