using System;
using System.Runtime.CompilerServices;

namespace Fibrous;

internal static class QueueSize
{
    internal const int DefaultQueueSize = 1008;
}

internal sealed class ArrayQueue<T>(int size)
{
    public static readonly (int, T[]) Empty = (0, Array.Empty<T>());
    private T[] _actions = new T[size + 16];
    private int _processCount;
    private T[] _toPass = new T[size + 16];

    public int Count { get; private set; }

    public bool IsFull => Count >= size;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Enqueue(T a)
    {
        int index0 = Count++;
        _actions[index0] = a;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (int count, T[] actions) Drain()
    {
        int processCount = Count;
        if (processCount == 0)
        {
            return Empty;
        }

        Swap(ref _actions, ref _toPass);
        int old = _processCount;
        _processCount = processCount;
        Array.Clear(_actions, 0, old);
        Count = 0;
        return (_processCount, _toPass);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Swap(ref T[] a, ref T[] b)
    {
        (a, b) = (b, a);
    }
}
