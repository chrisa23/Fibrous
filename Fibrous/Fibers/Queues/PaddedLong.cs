namespace Fibrous.Fibers.Queues
{
    using System;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    [StructLayout(LayoutKind.Explicit, Size = CachePadding.CacheLineSize * 2)]
    public struct PaddedLong
    {
        [FieldOffset(CachePadding.CacheLineSize)]
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

        public void LazySet(long value)
        {
            WriteCompilerOnlyFence(value);
        }

        public long Value { get { return ReadFullFence(); } set { WriteFullFence(value); } }

        /// <summary>
        /// Atomically set the value to the given updated value if the current value equals the comparand
        /// </summary>
        /// <param name="newValue">The new value</param>
        /// <param name="comparand">The comparand (expected value)</param>
        /// <returns></returns>
        public bool CompareExchange(long newValue, long comparand)
        {
            return Interlocked.CompareExchange(ref _value, newValue, comparand) == comparand;
        }

        /// <summary>
        /// Atomically set the value to the given updated value
        /// </summary>
        /// <param name="newValue">The new value</param>
        /// <returns>The original value</returns>
        public long Exchange(long newValue)
        {
            return Interlocked.Exchange(ref _value, newValue);
        }

        /// <summary>
        /// Atomically add the given value to the current value and return the sum
        /// </summary>
        /// <param name="delta">The value to be added</param>
        /// <returns>The sum of the current value and the given value</returns>
        public long Add(long delta)
        {
            return Interlocked.Add(ref _value, delta);
        }

        /// <summary>
        /// Atomically increment the current value and return the new value
        /// </summary>
        /// <returns>The incremented value.</returns>
        public long Increment()
        {
            return Interlocked.Increment(ref _value);
        }

        /// <summary>
        /// Atomically increment the current value and return the new value
        /// </summary>
        /// <returns>The decremented value.</returns>
        public long Decrement()
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
        public int Length
        {
            get { return _array.Length; }
        }

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
            var value = _array[index];
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
            var value = _array[index];
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
        public int Length
        {
            get { return _array.Length; }
        }

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
            var value = _array[index];
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
            var value = _array[index];
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
            var newValueInt = ToInt(newValue);
            var comparandInt = ToInt(comparand);
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
            var result = Interlocked.Exchange(ref _array[index], ToInt(newValue));
            return ToBool(result);
        }

        private static bool ToBool(int value)
        {
            if (value != False && value != True)
            {
                throw new ArgumentOutOfRangeException("value");
            }

            return value == True;
        }

        private static int ToInt(bool value)
        {
            return value ? True : False;
        }
    }

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
        public int Length
        {
            get { return _array.Length; }
        }

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
            var value = _array[index];
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
            var value = _array[index];
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