namespace Fibrous.Queues
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    //TODO:  deal with this and make it work with Pool and Thread fiber
    //add circular buffers (ala disruptor) and 
    public interface IQueue
    {
        void Enqueue(Action action);
        void Run();
        void Stop();
    }

    public interface IQueue<E>
    {
        /**
         * Return the next pooled mutable object that can be used by the producer.
         * 
         * @return the next mutable object that can be used by the producer.
         */
        E NextToOffer();
        /**
         * Offer an object to the queue. The object must have been previously obtained by calling <i>nextToOffer()</i>.
         * 
         * @param e the object to be offered to the queue.
         */
        void Offer(E e);
        /**
         * Return the number of objects that can be safely polled from this queue. It can return zero.
         * 
         * @return number of objects that can be polled.
         */
        long Available();
        /**
         * Poll a object from the queue. You can only call this method after calling <i>available()</i> so you
         * know what is the maximum times you can call it.
         * 
         * NOTE: You should NOT keep your own reference for this mutable object. Read what you need to get from it and release its reference.
         * 
         * @return an object from the queue.
         */
        E Poll();
        /**
         * Called to indicate that all polling have been done.
         */
        void Done();
    }

    /// <summary>
    /// A long value that may be updated atomically and is guaranteed to live on its own cache line (to prevent false sharing)
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 64 * 2)]
    public struct PaddedLong
    {
        [FieldOffset(64)]
        private long _value;

        /// <summary>
        /// Create a new <see cref="PaddedLong"/> with the given initial value.
        /// </summary>
        /// <param name="value">Initial value</param>
        public PaddedLong(long value)
        {
            _value = value;
        }

        /// <summary>
        /// Read the value without applying any fence
        /// </summary>
        /// <returns>The current value</returns>
        public long ReadUnfenced()
        {
            return _value;
        }

        /// <summary>
        /// Read the value applying acquire fence semantic
        /// </summary>
        /// <returns>The current value</returns>
        public long ReadAcquireFence()
        {
            long value = _value;
            Thread.MemoryBarrier();
            return value;
        }

        /// <summary>
        /// Read the value applying full fence semantic
        /// </summary>
        /// <returns>The current value</returns>
        public long ReadFullFence()
        {
            //??
            Thread.MemoryBarrier();
            return _value;
        }

        /// <summary>
        /// Read the value applying a compiler only fence, no CPU fence is applied
        /// </summary>
        /// <returns>The current value</returns>
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public long ReadCompilerOnlyFence()
        {
            return _value;
        }

        /// <summary>
        /// Write the value applying release fence semantic
        /// </summary>
        /// <param name="newValue">The new value</param>
        public void WriteReleaseFence(long newValue)
        {
            Thread.MemoryBarrier();
            _value = newValue;
        }

        /// <summary>
        /// Write the value applying full fence semantic
        /// </summary>
        /// <param name="newValue">The new value</param>
        public void WriteFullFence(long newValue)
        {
            Thread.MemoryBarrier();
            _value = newValue;
        }

        /// <summary>
        /// Write the value applying a compiler fence only, no CPU fence is applied
        /// </summary>
        /// <param name="newValue">The new value</param>
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public void WriteCompilerOnlyFence(long newValue)
        {
            _value = newValue;
        }

        /// <summary>
        /// Write without applying any fence
        /// </summary>
        /// <param name="newValue">The new value</param>
        public void WriteUnfenced(long newValue)
        {
            _value = newValue;
        }

        /// <summary>
        /// Atomically set the value to the given updated value if the current value equals the comparand
        /// </summary>
        /// <param name="newValue">The new value</param>
        /// <param name="comparand">The comparand (expected value)</param>
        /// <returns></returns>
        public bool AtomicCompareExchange(long newValue, long comparand)
        {
            return Interlocked.CompareExchange(ref _value, newValue, comparand) == comparand;
        }

        /// <summary>
        /// Atomically set the value to the given updated value
        /// </summary>
        /// <param name="newValue">The new value</param>
        /// <returns>The original value</returns>
        public long AtomicExchange(long newValue)
        {
            return Interlocked.Exchange(ref _value, newValue);
        }

        /// <summary>
        /// Atomically add the given value to the current value and return the sum
        /// </summary>
        /// <param name="delta">The value to be added</param>
        /// <returns>The sum of the current value and the given value</returns>
        public long AtomicAddAndGet(long delta)
        {
            return Interlocked.Add(ref _value, delta);
        }

        /// <summary>
        /// Atomically increment the current value and return the new value
        /// </summary>
        /// <returns>The incremented value.</returns>
        public long AtomicIncrementAndGet()
        {
            return Interlocked.Increment(ref _value);
        }

        /// <summary>
        /// Atomically increment the current value and return the new value
        /// </summary>
        /// <returns>The decremented value.</returns>
        public long AtomicDecrementAndGet()
        {
            return Interlocked.Decrement(ref _value);
        }

        /// <summary>
        /// Returns the string representation of the current value.
        /// </summary>
        /// <returns>the string representation of the current value.</returns>
        public override string ToString()
        {
            long value = ReadFullFence();
            return value.ToString();
        }
    }

    //
    public class AtomicQueue<E> : IQueue<E> where E : class
    {
        private const int DEFAULT_CAPACITY = 1024 * 16;
        private readonly int capacity;
        private readonly E[] data;
        private long nextOfferSequence = -1;
        private long nextPollSequence = -1;
        private long maxSequence;
        private PaddedLong offerSequence = new PaddedLong(-1);
        private PaddedLong pollSequence = new PaddedLong(-1);

        public AtomicQueue(int capacity, Func<E> builder)
        {
            //NumberUtils.ensurePowerOfTwo(capacity);
            this.capacity = capacity;
            data = new E[capacity];
            for (int i = 0; i < capacity; i++)
            {
                data[i] = builder();
            }
            maxSequence = FindMaxSeqBeforeWrapping();
        }

        public AtomicQueue(Func<E> builder)
            : this(DEFAULT_CAPACITY, builder)
        {
        }

        private long FindMaxSeqBeforeWrapping()
        {
            return capacity + pollSequence.ReadFullFence();
        }

        public E NextToOffer()
        {
            if (++nextOfferSequence > maxSequence)
            {
                // this would wrap the buffer... calculate the new one...
                maxSequence = FindMaxSeqBeforeWrapping();
                if (nextOfferSequence > maxSequence)
                {
                    nextOfferSequence--;
                    return null;
                }
            }
            return data[(int)(nextOfferSequence & capacity - 1)];
        }

        public void Offer(E e)
        {
            offerSequence.WriteCompilerOnlyFence(nextOfferSequence);
        }

        public long Available()
        {
            return offerSequence.ReadFullFence() - nextPollSequence;
        }

        public E Poll()
        {
            return data[(int)(++nextPollSequence & capacity - 1)];
        }

        public void Done()
        {
            pollSequence.WriteCompilerOnlyFence(nextPollSequence);
        }
    }
}