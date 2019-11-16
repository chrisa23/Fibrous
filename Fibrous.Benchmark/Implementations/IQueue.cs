namespace Fibrous
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Queue for fibers.  Drain always grabs full batch.
    /// </summary>
    public interface IQueue : IDisposable
    {
        void Enqueue(Action action);
        //Switch to ReadOnlySpan?
        List<Action> Drain();
    }

    public interface IQueue2
    {
        void Enqueue(Action action);
        //Switch to ReadOnlySpan?
    //    ReadOnlySpan<Action> Drain();

        event Action Signal;
    }

    public class CircularBufferQueue:IQueue2
    {
        protected static readonly int _bufferPad = 128 / IntPtr.Size;

        private Action[] _items;
        private int _indexMask;
        private readonly int[] _availableBuffer;
        private readonly int _indexShift;
        public CircularBufferQueue(int size)
        {
            _indexMask = size - 1;
            _availableBuffer = new int[size];
            _items = new Action[size + 2 * _bufferPad];
            _indexShift = Log2(size);
        }

        public static int Log2(int i)
        {
            var r = 0;
            while ((i >>= 1) != 0)
            {
                ++r;
            }
            return r;
        }

        public void Enqueue(Action action)
        {

            Signal?.Invoke();//this is for blocking and pool
        }

        //public ReadOnlySpan<Action> Drain()
        //{
        //    //needs wait strategy 

        //    return ReadOnlySpan<Action>.Empty;
        //}

        public event Action Signal;
    }
}