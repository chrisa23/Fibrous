namespace Fibrous.Experimental
{
    using System;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;

    /// <summary>
    /// A <see cref="bool"/> array that may be updated atomically
    /// </summary>
    public class BooleanArray
    {
        private readonly int[] _array;
        private const int False = 0;
        private const int True = 1;

        /// <summary>
        /// Create a new <see cref="BooleanArray"/> of a given length
        /// </summary>
        /// <param name="length">Length of the array</param>
        public BooleanArray(int length)
        {
            if (length <= 0) throw new ArgumentOutOfRangeException("length");
            _array = new int[length];
        }

        /// <summary>
        ///  Create a new <see cref="BooleanArray"/>with the same length as, and all elements copied from, the given array.
        /// </summary>
        /// <param name="array"></param>
        public BooleanArray(bool[] array)
        {
            if (array == null) throw new ArgumentNullException("array");
            _array = array.Select(ToInt).ToArray();
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
        public bool ReadUnfenced(int index)
        {
            return ToBool(_array[index]);
        }

        /// <summary>
        /// Read the value applying acquire fence semantic
        /// </summary>
        /// <param name="index">The element index</param>
        /// <returns>The current value</returns>
        public bool ReadAcquireFence(int index)
        {
            int value = _array[index];
            Thread.MemoryBarrier();
            return ToBool(value);
        }

        /// <summary>
        /// Read the value applying full fence semantic
        /// </summary>
        /// <param name="index">The element index</param>
        /// <returns>The current value</returns>
        public bool ReadFullFence(int index)
        {
            int value = _array[index];
            Thread.MemoryBarrier();
            return ToBool(value);
        }

        /// <summary>
        /// Read the value applying a compiler only fence, no CPU fence is applied
        /// </summary>
        /// <param name="index">The element index</param>
        /// <returns>The current value</returns>
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public bool ReadCompilerOnlyFence(int index)
        {
            return ToBool(_array[index]);
        }

        /// <summary>
        /// Write the value applying release fence semantic
        /// </summary>
        /// <param name="index">The element index</param>
        /// <param name="newValue">The new value</param>
        public void WriteReleaseFence(int index, bool newValue)
        {
            _array[index] = ToInt(newValue);
            Thread.MemoryBarrier();
        }

        /// <summary>
        /// Write the value applying full fence semantic
        /// </summary>
        /// <param name="index">The element index</param>
        /// <param name="newValue">The new value</param>
        public void WriteFullFence(int index, bool newValue)
        {
            _array[index] = ToInt(newValue);
            Thread.MemoryBarrier();
        }

        /// <summary>
        /// Write the value applying a compiler fence only, no CPU fence is applied
        /// </summary>
        /// <param name="index">The element index</param>
        /// <param name="newValue">The new value</param>
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public void WriteCompilerOnlyFence(int index, bool newValue)
        {
            _array[index] = ToInt(newValue);
        }

        /// <summary>
        /// Write without applying any fence
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="newValue">The new value</param>
        public void WriteUnfenced(int index, bool newValue)
        {
            _array[index] = ToInt(newValue);
        }

        /// <summary>
        /// Atomically set the value to the given updated value if the current value equals the comparand
        /// </summary>
        /// <param name="newValue">The new value</param>
        /// <param name="comparand">The comparand (expected value)</param>
        /// <param name="index">The index.</param>
        /// <returns>The original value</returns>
        public bool AtomicCompareExchange(int index, bool newValue, bool comparand)
        {
            int newValueInt = ToInt(newValue);
            int comparandInt = ToInt(comparand);
            return Interlocked.CompareExchange(ref _array[index], newValueInt, comparandInt) == comparandInt;
        }

        /// <summary>
        /// Atomically set the value to the given updated value
        /// </summary>
        /// <param name="newValue">The new value</param>
        /// <param name="index">The index.</param>
        /// <returns>The original value</returns>
        public bool AtomicExchange(int index, bool newValue)
        {
            int result = Interlocked.Exchange(ref _array[index], ToInt(newValue));
            return ToBool(result);
        }

        private static bool ToBool(int value)
        {
            if (value != False && value != True)
                throw new ArgumentOutOfRangeException("value");
            return value == True;
        }

        private static int ToInt(bool value)
        {
            return value ? True : False;
        }
    }
}