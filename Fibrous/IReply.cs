namespace Fibrous
{
    using System;

    public interface IReply<T>
    {
        Result<T> Receive(TimeSpan timeout);
    }

    public struct Result<T>
    {
        public bool IsValid { get; set; }
        public T Value { get; set; }

        public Result(T value)
            : this()
        {
            Value = value;
            IsValid = true;
        }
    }
}