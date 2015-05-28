namespace Fibrous
{
    public struct Result<T> : IResult<T>
    {
        public Result(T value)
            : this()
        {
            Value = value;
            IsValid = true;
        }

        public bool IsValid { get; private set; }
        public T Value { get; private set; }
    }
}