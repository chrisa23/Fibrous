using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Fibrous.Internal;

namespace Fibrous
{
    internal interface IQueue<T>
    {
        bool IsFull { get; }
        int Count { get; }
        void Enqueue(T a);
        (int, T[]) Drain();
    }

    internal static class QueueSize
    {
        internal const int DefaultQueueSize = 1008;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    internal class ArrayQueue<T> : IQueue<T>
    {
        
        private readonly int _size;
        //Pad112 p01;
        private T[] _actions;
        //Pad112 p02;
        private T[] _toPass;
        //Pad120 p03;
        private int _processCount;

        Pad56 p0;

        private int _count;

        Pad56 p1;


        public ArrayQueue(int size)
        {
            _size = size;
            _actions = new T[size + 16];
            _toPass = new T[size + 16];
        }

        public int Count =>  _count;

        public bool IsFull => _count >= _size;

        public void Enqueue(T a)
        {
            var index0 = _count++;
            _actions[index0] = a;
        }

        public (int, T[]) Drain()
        {
            int processCount = _count;
            if (processCount == 0) return Empty;
            Swap(ref _actions, ref _toPass);
            var old = _processCount;
            _processCount = processCount;
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