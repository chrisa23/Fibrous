namespace Fibrous.Fibers.Queues
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;

    public class ActionQueue : IQueue
    {
        private const int DefaultCapacity = 1024 * 16;
        private readonly int _capacity;
        //   private readonly int _batchsize;
        private readonly Action[] _data;
        private long _nextOfferSequence = -1;
        private long _nextPollSequence = -1;
        private long _maxSequence;
        private PaddedLong _offerSequence = new PaddedLong(-1);
        private PaddedLong _pollSequence = new PaddedLong(-1);
        private readonly int _indexMask;
        //  private readonly Batch _batch;
        public ActionQueue(int capacity) //, int batchsize = 1024)
        {
            //NumberUtils.ensurePowerOfTwo(capacity);
            _capacity = capacity;
            //  _batchsize = batchsize;
            // _batch = new Batch(batchsize);
            _data = new Action[capacity];
            _maxSequence = FindMaxSeqBeforeWrapping();
            _indexMask = _capacity - 1;
        }

        private long FindMaxSeqBeforeWrapping()
        {
            return _capacity + _pollSequence.Value;
        }

        //public Action Poll()
        //{
        //    Action result = null;
        //    long avail = Available();
        //    if (avail > 0)
        //    {
        //        var index0 = (int)(++_nextPollSequence & _indexMask);
        //        ActionHolder actionHolder = _data[index0];
        //        result = actionHolder.Action;
        //        actionHolder.Action = null;
        //    }
        //    return result;
        //    //long take = avail > _batchsize ? _batchsize : avail;
        //    //    for (int i = 0; i < take; i++)
        //    //    {
        //    //        var index0 = (int)(++_nextPollSequence & _indexMask);
        //    //        ActionHolder actionHolder = _data[index0];
        //    //        _batch.Actions[i] = actionHolder.Action;
        //    //        actionHolder.Action = null;
        //    //    }
        //    //    _batch.Count = take;
        //    //    _pollSequence.LazySet(_nextPollSequence);
        //    //}
        //    //return _batch;
        //}
        private void Done()
        {
            _pollSequence.LazySet(_nextPollSequence);
        }

        public long Available()
        {
            return _offerSequence.ReadFullFence() - _nextPollSequence;
        }

        public void Enqueue(Action action)
        {
            if (++_nextOfferSequence > _maxSequence)
            {
                // this would wrap the buffer... calculate the new one...
                while ((_maxSequence = FindMaxSeqBeforeWrapping()) < _nextOfferSequence)
                    //if (_nextOfferSequence > _maxSequence)
                {
                    //_nextOfferSequence--;
                    Thread.Sleep(0);
                }
            }
            _data[(int)(_nextOfferSequence & _indexMask)] = action;
            _offerSequence.Value = _nextOfferSequence;
        }

        public bool HasItems()
        {
            return Available() > 0;
        }

        //do this without allocation...
        public IEnumerable<Action> DequeueAll()
        {
            long avail = Available();
            var results = new Action[avail];
            if (avail > 0)
            {
                for (long i = 0; i < avail; i++)
                {
                    var index0 = (int)(++_nextPollSequence & _indexMask);
                    Action action = _data[index0];
                    results[i] = action;
                    _data[index0] = null;
                }
                Done();
            }
            return results;
            //long take = avail > _batchsize ? _batchsize : avail;
            //    for (int i = 0; i < take; i++)
            //    {
            //        var index0 = (int)(++_nextPollSequence & _indexMask);
            //        ActionHolder actionHolder = _data[index0];
            //        _batch.Actions[i] = actionHolder.Action;
            //        actionHolder.Action = null;
            //    }
            //    _batch.Count = take;
            //    _pollSequence.LazySet(_nextPollSequence);
            //}
            //return _batch;
        }

        public class Enumerable : IEnumerable<Action>
        {
            public class Enumerator : IEnumerator<Action>
            {
                private long _cursor;
                private int _count;
                private int _current = -1;
                private int _indexMask;
                private Action[] _actions;
                public Action Current { get { return _actions[(int)((_cursor + _current) & _indexMask)]; } }

                public void Dispose()
                {
                }

                object IEnumerator.Current { get { return Current; } }

                public bool MoveNext()
                {
                    _current++;
                    return _current <= _count;
                }

                public void Reset()
                {
                }
            }

            public IEnumerator<Action> GetEnumerator()
            {
                return new Enumerator();
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