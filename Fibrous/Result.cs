namespace Fibrous
{
    public struct Result<T>
    {
        public bool IsValid;
        public readonly T Value;

        public Result(T value)
            : this()
        {
            Value = value;
            IsValid = true;
        }
    }
}