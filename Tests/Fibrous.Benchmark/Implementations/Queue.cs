using System;
using System.Collections.Generic;

namespace Fibrous
{
    public static class Queue
    {
        public static readonly List<Action> Empty = new List<Action>();
    }


    public class ArrayQueue
    {
        public static readonly (int, Action[]) Empty = (0, new Action[0]);
        private readonly int _maxIndex;
        private Action[] _actions;
        private int _processCount;
        private Action[] _toPass;

        public ArrayQueue(int size)
        {
            _maxIndex = size - 1;
            _actions = new Action[size];
            _toPass = new Action[size];
        }

        public int Count { get; private set; }

        public bool IsFull => Count >= _maxIndex;

        public void Enqueue(Action a)
        {
            _actions[Count++] = a;
        }

        public (int, Action[]) Drain()
        {
            if (Count == 0) return Empty;
            Swap(ref _actions, ref _toPass);
            var old = _processCount;
            _processCount = Count;
            Array.Clear(_actions, 0, old);
            Count = 0;
            return (_processCount, _toPass);
        }

        public static void Swap(ref Action[] a, ref Action[] b)
        {
            var tmp = a;
            a = b;
            b = tmp;
        }
    }

    public class ArrayQueue<T>
    {
        public static readonly (int, T[]) Empty = (0, new T[0]);
        private readonly int _maxIndex;
        private T[] _actions;
        private int _processCount;
        private T[] _toPass;

        public ArrayQueue(int size)
        {
            _maxIndex = size - 1;
            _actions = new T[size];
            _toPass = new T[size];
        }

        public int Count { get; private set; }

        public bool IsFull => Count >= _maxIndex;

        public void Enqueue(T a)
        {
            _actions[Count++] = a;
        }

        public (int, T[]) Drain()
        {
            if (Count == 0) return Empty;
            Swap(ref _actions, ref _toPass);
            var old = _processCount;
            _processCount = Count;
            Array.Clear(_actions, 0, old);
            Count = 0;
            return (_processCount, _toPass);
        }

        public static void Swap(ref T[] a, ref T[] b)
        {
            var tmp = a;
            a = b;
            b = tmp;
        }
    }
}