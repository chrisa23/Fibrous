using System.Threading;

namespace Fibrous
{
    using System;
    using System.Collections.Generic;

    public static class Queue
    {
        public static readonly List<Action> Empty = new List<Action>();
    }


    public class ArrayQueue
    {
        private readonly int _maxIndex;
        private Action[] _actions;
        private Action[] _toPass;
        private int _actionCount = 0;
        private int _processCount = 0;

        public int Count => _actionCount;
        public bool IsFull => _actionCount >= _maxIndex;

        public ArrayQueue(int size)
        {
            _maxIndex = size - 1;
            _actions = new Action[size];
            _toPass = new Action[size];
        }

        public void Enqueue(Action a)
        {
            _actions[_actionCount++] = a;
        }

        public static readonly (int, Action[]) Empty = (0, new Action[0]);

        public (int, Action[]) Drain()
        {
            if (_actionCount == 0) return Empty;
            Swap(ref _actions, ref _toPass);
            int old = _processCount;
            _processCount = _actionCount;
            Array.Clear(_actions, 0, old);
            _actionCount = 0;
            return (_processCount, _toPass);
        }

        public static void Swap(ref Action[] a, ref Action[] b)
        {
            Action[] tmp = a;
            a = b;
            b = tmp;
        }
    }

    public class ArrayQueue<T>
    {
        private readonly int _maxIndex;
        private T[] _actions;
        private T[] _toPass;
        private int _actionCount = 0;
        private int _processCount = 0;

        public int Count => _actionCount;
        public bool IsFull => _actionCount >= _maxIndex;

        public ArrayQueue(int size)
        {
            _maxIndex = size - 1;
            _actions = new T[size];
            _toPass = new T[size];
        }

        public void Enqueue(T a)
        {
            _actions[_actionCount++] = a;
        }

        public static readonly (int, T[]) Empty = (0, new T[0]);

        public (int, T[]) Drain()
        {
            if (_actionCount == 0) return Empty;
            Swap(ref _actions, ref _toPass);
            int old = _processCount;
            _processCount = _actionCount;
            Array.Clear(_actions, 0, old);
            _actionCount = 0;
            return (_processCount, _toPass);
        }

        public static void Swap(ref T[] a, ref T[] b)
        {
            T[] tmp = a;
            a = b;
            b = tmp;
        }
    }
}