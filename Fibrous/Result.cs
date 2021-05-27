namespace Fibrous
{
    public readonly struct Result<T>
    {
        public readonly T Value;
        public readonly bool Succeeded;
 
        private Result(T value)
        {
            Succeeded = true;
            Value = value;
        }

        public static Result<T> Ok(T value) => new Result<T>(value);
        public static readonly Result<T> Failed = default;
    }
}
