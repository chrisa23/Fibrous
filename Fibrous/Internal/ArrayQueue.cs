using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Fibrous
{
    internal static class QueueSize
    {
        internal const int DefaultQueueSize = 1008;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    internal sealed class ArrayQueue<T>
    {
        
        private readonly int _size;
        private T[] _actions;
        private T[] _toPass;
        private int _processCount;

        public ArrayQueue(int size)
        {
            _size = size;
            _actions = new T[size + 16];
            _toPass = new T[size + 16];
        }

        public int Count { get; private set; }

        public bool IsFull => Count >= _size;

        public void Enqueue(T a)
        {
            var index0 = Count++;
            _actions[index0] = a;
        }

        public (int count, T[] actions) Drain()
        {
            int processCount = Count;
            if (processCount == 0) return Empty;
            Swap(ref _actions, ref _toPass);
            var old = _processCount;
            _processCount = processCount;
            Array.Clear(_actions, 0, old);
            Count = 0;
            return (_processCount, _toPass);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Swap(ref T[] a, ref T[] b)
        {
            var tmp = a;
            a = b;
            b = tmp;
        }
        
        public static readonly (int, T[]) Empty = (0, new T[0]);

    }
}