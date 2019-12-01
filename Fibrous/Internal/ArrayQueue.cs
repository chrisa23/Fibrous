using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace Fibrous
{
    internal interface IQueue<T>
    {
        bool IsFull { get; }
        void Enqueue(T a);
        (int, T[]) Drain();
    }

    internal static class QueueSize
    {
        internal const int DefaultQueueSize = 1024;
    }
    internal class ArrayQueue<T> : IQueue<T>
    {
        private readonly int _size;
        private T[] _actions;
        private T[] _toPass;
        private int _processCount;
        private int _count;
    
        public ArrayQueue(int size)
        {
            _size = size;
            _actions = new T[size];
            _toPass = new T[size];
        }

        public int Count => _count;

        public bool IsFull => _count >= _size;

        public void Enqueue(T a)
        {
            _actions[_count++] = a;
        }

        public (int, T[]) Drain()
        {
            if (_count == 0) return Empty;
            Swap(ref _actions, ref _toPass);
            var old = _processCount;
            _processCount = _count;
            Array.Clear(_actions, 0, old);
            _count = 0;
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