namespace Fibrous.Fibers.Queues
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;

    public class ActionQueue : IQueue
    {
        private const int DefaultCapacity = 1024 * 1024;//1MB
        private readonly int _capacity;
        private readonly Action[] _data;
        //private long _nextOfferSequence = -1;
        //private long _nextPollSequence = -1;
        private long _maxSequence;
        private PaddedLong _offerSequence = new PaddedLong(-1);
        private PaddedLong _pollSequence = new PaddedLong(-1);
        private readonly int _indexMask;
        public ActionQueue(int capacity = DefaultCapacity)
        {
            //NumberUtils.ensurePowerOfTwo(capacity);
            _capacity = capacity;
            _data = new Action[capacity];
            _maxSequence = FindMaxSeqBeforeWrapping();
            _indexMask = _capacity - 1;
        }

        private long FindMaxSeqBeforeWrapping()
        {
            return _capacity + _pollSequence.Value;
        }

  
        public int Available()
        {
            return (int)(_offerSequence.ReadFullFence() - _pollSequence.ReadFullFence());
        }

        public void Enqueue(Action action)
        {
            long next = _offerSequence.Value;
            if (++next > _maxSequence)
            {
                // this would wrap the buffer... calculate the new one...
                while ((_maxSequence = FindMaxSeqBeforeWrapping()) < next)
                    //if (_nextOfferSequence > _maxSequence)
                {
                    //_nextOfferSequence--;
                    Thread.Sleep(1);//??
                }
            }
            _data[(int)(next & _indexMask)] = action;
            _offerSequence.Value = next;
        }

        public bool HasItems()
        {
            return Available() > 0;
        }

        //do this without allocation...
        public IEnumerable<Action> DequeueAll()
        {
            int avail = Available();
            if (avail == 0)
                return Queue.Empty;
            return new Enumerable(avail, _data, _indexMask, _pollSequence);
        }

        public struct Enumerable : IEnumerable<Action>
        {
            private readonly Enumerator _enumerator;

            public Enumerable(int count, Action[] actions, int indexMask, PaddedLong pollSequence)
            {
                _enumerator = new Enumerator(pollSequence.Value, count,actions,indexMask, pollSequence);
            }

            private struct Enumerator : IEnumerator<Action>
            {
                private PaddedLong _pollSequence;
                private readonly long _cursor;
                private readonly int _count;
                private int _current;
                private readonly int _indexMask;
                private readonly Action[] _actions;
                public Action Current { get { return _actions[(int)((_cursor + _current) & _indexMask)]; } }

                public Enumerator(long cursor, int count, Action[] actions, int indexMask, PaddedLong pollSequence) : this()
                {
                    _cursor = cursor;
                    _count = count;
                    _actions = actions;
                    _indexMask = indexMask;
                    _pollSequence = pollSequence;
                    _current = -1;
                }

                public void Dispose()
                {
                    _pollSequence.LazySet(_cursor + _count);
                }

                object IEnumerator.Current { get { return Current; } }

                public bool MoveNext()
                {
                    if (_current > -1 && _current < _count)
                        _actions[(int)((_cursor + _current) & _indexMask)] = null;
                    _current++;
                    return _current < _count;
                }

                public void Reset()
                {
                }
            }

            public IEnumerator<Action> GetEnumerator()
            {
                return _enumerator;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        public void Dispose()
        {
        }
    }
}