namespace Fibrous.Fibers.Queues
{
    using System;
    using System.Threading;

    public interface IQueue : IDisposable
    {
        void Enqueue(Action action);
        void Drain(IExecutor executor);
        //IEnumerable<Action> DequeueAll();
    }

    public sealed class DisruptorQueue : IQueue
    {
        private IWaitStrategy _wait = new YieldingWaitStrategy();
        public void Dispose()
        {

        }

        public void Enqueue(Action action)
        {
            long next = Next();
            _data[IndexOf(next)] = action;
            Publish(next);
            _wait.SignalWhenBlocking();
        }

        private readonly Action[] _data;
        private readonly int _size;
        private readonly int _mask;
        //private PaddedLong _nextSequence = new PaddedLong(-1);
        private PaddedLong _cursor = new PaddedLong(-1);
        private PaddedLong _readSequence = new PaddedLong(-1);
        private readonly IntegerArray _published;
        private readonly int _indexShift;

        public DisruptorQueue(int size)
        {
            _data = new Action[size];
            _size = size;
            _mask = size - 1;
            _published = new IntegerArray(size);
            _indexShift = log2(size);
            for (int i = 0; i < size; i++)
                _published.WriteUnfenced(i, -1);
        }

        private long Next()
        {
            long next;
            long current;
            do
            {
                current = _cursor.Value;
                next = current + 1;
                while (next > (_readSequence.Value + _size))
                {
                    //LockSupport.parkNanos(1L);
                    //Thread.Sleep(0);
                    Thread.Yield();
                    continue;
                }
            }
            while (!_cursor.CompareExchange(next, current));
            return next;
        }

        private void Publish(long sequence)
        {
            var publishedValue = (int)(sequence >> _indexShift);
            _published.WriteFullFence(IndexOf(sequence), publishedValue);
        }

        public void Drain(IExecutor handler)
        {
            long available = _cursor.Value;
            //bool hasSome = available > _readSequence.Value;
            for (long current = _readSequence.Value + 1; current <= available; current++)
            {
                var availableValue = (int)(current >> _indexShift);
                int index = IndexOf(current);
                while (_published.ReadFullFence(index) != availableValue)
                {
                    // Spin
                }
                int index0 = IndexOf(current);
                Action execute = _data[index0];
                _data[index0] = null;
                handler.Execute(execute);
            }
            
            _readSequence.LazySet(available);
            
            //if we have nothing left, wait
            _wait.Wait();
            //otherwise, cycle drain?
           
        }

        private int IndexOf(long sequence)
        {
            return ((int)sequence) & _mask;
        }

        /**
* Calculate the log base 2 of the supplied integer, essentially reports the location
* of the highest bit.
*
* @param i Value to calculate log2 for.
* @return The log2 value
*/

        public static int log2(int i)
        {
            int r = 0;
            while ((i >>= 1) != 0)
                ++r;
            return r;
        }
    }
}