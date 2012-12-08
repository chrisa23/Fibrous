namespace Fibrous.Experimental
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;

    /// <summary>
    /// A <see cref="long"/> array that may be updated atomically
    /// </summary>
    public class LongArray
    {
        private readonly long[] _array;

        /// <summary>
        /// Create a new <see cref="LongArray"/> of a given length
        /// </summary>
        /// <param name="length">Length of the array</param>
        public LongArray(int length)
        {
            if (length <= 0) throw new ArgumentOutOfRangeException("length");
            _array = new long[length];
        }

        /// <summary>
        ///  Create a new <see cref="LongArray"/>with the same length as, and all elements copied from, the given array.
        /// </summary>
        /// <param name="array"></param>
        public LongArray(long[] array)
        {
            if (array == null) throw new ArgumentNullException("array");
            _array = new long[array.Length];
            array.CopyTo(_array, 0);
        }

        /// <summary>
        /// Length of the array
        /// </summary>
        public int Length { get { return _array.Length; } }

        /// <summary>
        /// Read the value without applying any fence
        /// </summary>
        /// <param name="index">The index of the element.</param>
        /// <returns>The current value.</returns>
        public long ReadUnfenced(int index)
        {
            return _array[index];
        }

        /// <summary>
        /// Read the value applying acquire fence semantic
        /// </summary>
        /// <param name="index">The element index</param>
        /// <returns>The current value</returns>
        public long ReadAcquireFence(int index)
        {
            long value = _array[index];
            Thread.MemoryBarrier();
            return value;
        }

        /// <summary>
        /// Read the value applying full fence semantic
        /// </summary>
        /// <param name="index">The element index</param>
        /// <returns>The current value</returns>
        public long ReadFullFence(int index)
        {
            long value = _array[index];
            Thread.MemoryBarrier();
            return value;
        }

        /// <summary>
        /// Read the value applying a compiler only fence, no CPU fence is applied
        /// </summary>
        /// <param name="index">The element index</param>
        /// <returns>The current value</returns>
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public long ReadCompilerOnlyFence(int index)
        {
            return _array[index];
        }

        /// <summary>
        /// Write the value applying release fence semantic
        /// </summary>
        /// <param name="index">The element index</param>
        /// <param name="newValue">The new value</param>
        public void WriteReleaseFence(int index, long newValue)
        {
            _array[index] = newValue;
            Thread.MemoryBarrier();
        }

        /// <summary>
        /// Write the value applying full fence semantic
        /// </summary>
        /// <param name="index">The element index</param>
        /// <param name="newValue">The new value</param>
        public void WriteFullFence(int index, long newValue)
        {
            _array[index] = newValue;
            Thread.MemoryBarrier();
        }

        /// <summary>
        /// Write the value applying a compiler fence only, no CPU fence is applied
        /// </summary>
        /// <param name="index">The element index</param>
        /// <param name="newValue">The new value</param>
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public void WriteCompilerOnlyFence(int index, long newValue)
        {
            _array[index] = newValue;
        }

        /// <summary>
        /// Write without applying any fence
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="newValue">The new value</param>
        public void WriteUnfenced(int index, long newValue)
        {
            _array[index] = newValue;
        }

        /// <summary>
        /// Atomically set the value to the given updated value if the current value equals the comparand
        /// </summary>
        /// <param name="newValue">The new value</param>
        /// <param name="comparand">The comparand (expected value)</param>
        /// <param name="index">The index.</param>
        /// <returns>The original value</returns>
        public bool AtomicCompareExchange(int index, long newValue, long comparand)
        {
            return Interlocked.CompareExchange(ref _array[index], newValue, comparand) == comparand;
        }

        /// <summary>
        /// Atomically set the value to the given updated value
        /// </summary>
        /// <param name="newValue">The new value</param>
        /// <param name="index">The index.</param>
        /// <returns>The original value</returns>
        public long AtomicExchange(int index, long newValue)
        {
            return Interlocked.Exchange(ref _array[index], newValue);
        }

        /// <summary>
        /// Atomically add the given value to the current value and return the sum
        /// </summary>
        /// <param name="delta">The value to be added</param>
        /// <param name="index">The index.</param>
        /// <returns>The sum of the current value and the given value</returns>
        public long AtomicAddAndGet(int index, long delta)
        {
            return Interlocked.Add(ref _array[index], delta);
        }

        /// <summary>
        /// Atomically increment the current value and return the new value
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The incremented value.</returns>
        public long AtomicIncrementAndGet(int index)
        {
            return Interlocked.Increment(ref _array[index]);
        }

        /// <summary>
        /// Atomically increment the current value and return the new value
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The decremented value.</returns>
        public long AtomicDecrementAndGet(int index)
        {
            return Interlocked.Decrement(ref _array[index]);
        }
    }
}