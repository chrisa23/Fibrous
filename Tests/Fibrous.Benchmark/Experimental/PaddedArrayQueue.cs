//using System;
//using System.Collections.Generic;
//using System.Runtime.CompilerServices;
//using System.Runtime.InteropServices;
//using System.Text;

//namespace Fibrous.Internal
//{
//    [StructLayout(LayoutKind.Explicit, Size = 320)]
//    public abstract class PaddedArrayQueue
//    {
//        [FieldOffset(56)]
//        protected object _actions;

//        [FieldOffset(120)]
//        protected object _toPass;

//        [FieldOffset(188)]
//        protected int _size;

//        [FieldOffset(188 + 64)]
//        protected int _processCount;

//        [FieldOffset(188 + 64 * 2)]
//        protected int _count;
//    }

//    internal class PaddedArrayQueue<T> : PaddedArrayQueue, IQueue<T>
//    {
//        public PaddedArrayQueue(int size)
//        {
//            _size = size;
//            _actions = new T[size];
//            _toPass = new T[size];
//        }

//        public int Count => _count;

//        public bool IsFull => _count >= _size;

//        public void Enqueue(T a)
//        {
//            _actions[_count++] = a;
//        }

//        public (int, T[]) Drain()
//        {
//            if (_count == 0) return Empty;
//            Swap(ref _actions, ref _toPass);
//            var old = _processCount;
//            _processCount = _count;
//            Array.Clear(_actions, 0, old);
//            _count = 0;
//            return (_processCount, _toPass);
//        }

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public static void Swap(ref object a, ref object b)
//        {
//            var tmp = a;
//            a = b;
//            b = tmp;
//        }

//        public static readonly (int, T[]) Empty = (0, new T[0]);

//    }
//}


