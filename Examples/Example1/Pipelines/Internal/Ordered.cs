namespace Example1.Pipelines.Internal;

internal struct Ordered<T>
{
    public Ordered(long index, T item)
    {
        Index = index;
        Item = item;
    }

    public long Index { get; set; }
    public T Item { get; set; }
}
