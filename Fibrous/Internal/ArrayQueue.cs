using System;
using System.Runtime.CompilerServices;

namespace Fibrous
{
    internal static class QueueSize
    {
        internal const int DefaultQueueSize = 1008;
    }

    internal sealed class ArrayQueue<T>
    {
        public static readonly (int, T[]) Empty = (0, new T[0]);
        private readonly int _size;
        private T[] _actions;
        private volatile int _count;
        private int _processCount;
        private T[] _toPass;

        public ArrayQueue(int size)
        {
            _size = size;
            _actions = new T[size + 16];
            _toPass = new T[size + 16];
        }

        public int Count => _count;

        public bool IsFull => Count >= _size;

        public void Enqueue(T a)
        {
            int index0 = _count++;
            _actions[index0] = a;
        }

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
            _count = 0;
            return (_processCount, _toPass);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Swap(ref T[] a, ref T[] b)
        {
            T[] tmp = a;
            a = b;
            b = tmp;
        }
    }
}
