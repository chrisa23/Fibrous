namespace Fibrous.Experimental
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;

    /// <summary>
    /// An <see cref="int"/> array that may be updated atomically
    /// </summary>
    public class IntegerArray
    {
        private readonly int[] _array;

        /// <summary>
        /// Create a new <see cref="IntegerArray"/> of a given length
        /// </summary>
        /// <param name="length">Length of the array</param>
        public IntegerArray(int length)
        {
            if (length <= 0) throw new ArgumentOutOfRangeException("length");
            _array = new int[length];
        }

        /// <summary>
        ///  Create a new AtomicIntegerArray with the same length as, and all elements copied from, the given array.
        /// </summary>
        /// <param name="array"></param>
        public IntegerArray(int[] array)
        {
            if (array == null) throw new ArgumentNullException("array");
            _array = new int[array.Length];
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
        public int ReadUnfenced(int index)
        {
            return _array[index];
        }

        /// <summary>
        /// Read the value applying acquire fence semantic
        /// </summary>
        /// <param name="index">The element index</param>
        /// <returns>The current value</returns>
        public int ReadAcquireFence(int index)
        {
            int value = _array[index];
            Thread.MemoryBarrier();
            return value;
        }

        /// <summary>
        /// Read the value applying full fence semantic
        /// </summary>
        /// <param name="index">The element index</param>
        /// <returns>The current value</returns>
        public int ReadFullFence(int index)
        {
            int value = _array[index];
            Thread.MemoryBarrier();
            return value;
        }

        /// <summary>
        /// Read the value applying a compiler only fence, no CPU fence is applied
        /// </summary>
        /// <param name="index">The element index</param>
        /// <returns>The current value</returns>
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public int ReadCompilerOnlyFence(int index)
        {
            return _array[index];
        }

        /// <summary>
        /// Write the value applying release fence semantic
        /// </summary>
        /// <param name="index">The element index</param>
        /// <param name="newValue">The new value</param>
        public void WriteReleaseFence(int index, int newValue)
        {
            _array[index] = newValue;
            Thread.MemoryBarrier();
        }

        /// <summary>
        /// Write the value applying full fence semantic
        /// </summary>
        /// <param name="index">The element index</param>
        /// <param name="newValue">The new value</param>
        public void WriteFullFence(int index, int newValue)
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
        public void WriteCompilerOnlyFence(int index, int newValue)
        {
            _array[index] = newValue;
        }

        /// <summary>
        /// Write without applying any fence
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="newValue">The new value</param>
        public void WriteUnfenced(int index, int newValue)
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
        public bool AtomicCompareExchange(int index, int newValue, int comparand)
        {
            return Interlocked.CompareExchange(ref _array[index], newValue, comparand) == comparand;
        }

        /// <summary>
        /// Atomically set the value to the given updated value
        /// </summary>
        /// <param name="newValue">The new value</param>
        /// <param name="index">The index.</param>
        /// <returns>The original value</returns>
        public int AtomicExchange(int index, int newValue)
        {
            return Interlocked.Exchange(ref _array[index], newValue);
        }

        /// <summary>
        /// Atomically add the given value to the current value and return the sum
        /// </summary>
        /// <param name="delta">The value to be added</param>
        /// <param name="index">The index.</param>
        /// <returns>The sum of the current value and the given value</returns>
        public int AtomicAddAndGet(int index, int delta)
        {
            return Interlocked.Add(ref _array[index], delta);
        }

        /// <summary>
        /// Atomically increment the current value and return the new value
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The incremented value.</returns>
        public int AtomicIncrementAndGet(int index)
        {
            return Interlocked.Increment(ref _array[index]);
        }

        /// <summary>
        /// Atomically increment the current value and return the new value
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The decremented value.</returns>
        public int AtomicDecrementAndGet(int index)
        {
            return Interlocked.Decrement(ref _array[index]);
        }
    }
}