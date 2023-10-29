namespace Fibrous;

public readonly struct Reply<T>
{
    public readonly T Value;
    public readonly bool Succeeded;

    private Reply(T value)
    {
        Succeeded = true;
        Value = value;
    }

    public static Reply<T> Ok(T value) => new(value);
    public static readonly Reply<T> Failed = default;
}
