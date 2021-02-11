namespace Fibrous
{
    public struct Result<T>
    {
        public T Value { get; private set; }
        public bool Succeeded { get; private set; }

        public static Result<T> Ok(T value) => new Result<T> {Succeeded = true, Value = value};
        public static readonly Result<T> Failed = new Result<T> {Succeeded = false};
    }
}
