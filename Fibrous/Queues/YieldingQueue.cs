namespace Fibrous.Experimental
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using Fibrous.Queues;

    public sealed class YieldingQueue : IQueue 
    {
        private List<Action> Actions = new List<Action>(1024);
        private List<Action> ToPass = new List<Action>(1024);

        private const int SpinTries = 100;
        private PaddedBoolean _signalled = new PaddedBoolean(false);

        private void Wait()
        {
            int counter = SpinTries;
            while (!_signalled.Value) // volatile read
                counter = ApplyWaitMethod(counter);
            _signalled.Exchange(false);
        }

        private static int ApplyWaitMethod(int counter)
        {
            if (counter == 0)
                Thread.Sleep(0);
            else
                --counter;
            return counter;
        }

        private readonly object _syncRoot = new object();

        public  void Enqueue(Action action)
        {
            lock (_syncRoot)
            {
                Actions.Add(action);
            }
            _signalled.LazySet(true);
        }

        public List<Action> Drain()
        {
            return DequeueAll();
        }

        public int Count { get { return Actions.Count; } }

        private List<Action> DequeueAll()
        {
            Wait();
            lock (_syncRoot)
            {
                if (Actions.Count == 0) return Queue.Empty;
                Lists.Swap(ref Actions, ref ToPass);
                Actions.Clear();
                return ToPass;
            }
        }

        public void Dispose()
        {
            _signalled.Exchange(true);
        }
    }


    public sealed class SleepingQueue : IQueue
    {
        protected List<Action> Actions = new List<Action>(1024);
        protected List<Action> ToPass = new List<Action>(1024);

        private const int SpinTries = 100;
        private PaddedBoolean _signalled = new PaddedBoolean(false);
        private readonly object _syncRoot = new object();
        private readonly TimeSpan _timeout = TimeSpan.FromMilliseconds(100);

        public void Wait()
        {
            SpinWait spinWait = default(SpinWait);
            Stopwatch sw = Stopwatch.StartNew();
            while (!_signalled.Value) // volatile read
            {
                spinWait.SpinOnce();
                if (sw.Elapsed > _timeout)
                    break;
            }
            _signalled.Exchange(false);
        }

        public void Enqueue(Action action)
        {
            lock (_syncRoot)
            {
                Actions.Add(action);
            }
            _signalled.LazySet(true);
        }

        public List<Action> Drain()
        {
            return DequeueAll();
        }

        public int Count { get { return Actions.Count; } }

        private List<Action> DequeueAll()
        {
            Wait();
            lock (_syncRoot)
            {
                if (Actions.Count == 0) return Queue.Empty;
                Lists.Swap(ref Actions, ref ToPass);
                Actions.Clear();
                return ToPass;
            }
        }

        public void Dispose()
        {
            lock (_syncRoot)
            {
                Monitor.PulseAll(_syncRoot);
            }
            _signalled.LazySet(true);
        }
    }

    ///<summary>
    /// Size of a cache line in bytes
    ///</summary>
    public static class CachePadding
    {
        public const int CacheLineSize = 64;
    }
    /// <summary>
    /// A boolean value that may be updated atomically and is guaranteed to live on its own cache line (to prevent false sharing)
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = CachePadding.CacheLineSize * 2)]
    public struct PaddedBoolean
    {
        // bool stored as an int, CAS not available on bool
        [FieldOffset(CachePadding.CacheLineSize)]
        private int _value;
        private const int False = 0;
        private const int True = 1;

        /// <summary>
        /// Create a new <see cref="PaddedBoolean"/> with the given initial value.
        /// </summary>
        /// <param name="value">Initial value</param>
        public PaddedBoolean(bool value)
        {
            _value = value ? True : False;
        }

        /// <summary>
        /// Read the value without applying any fence
        /// </summary>
        /// <returns>The current value</returns>
        public bool ReadUnfenced()
        {
            return ToBool(_value);
        }

        /// <summary>
        /// Read the value applying acquire fence semantic
        /// </summary>
        /// <returns>The current value</returns>
        public bool ReadAcquireFence()
        {
            bool value = ToBool(_value);
            Thread.MemoryBarrier();
            return value;
        }

        /// <summary>
        /// Read the value applying full fence semantic
        /// </summary>
        /// <returns>The current value</returns>
        public bool ReadFullFence()
        {
            bool value = ToBool(_value);
            Thread.MemoryBarrier();
            return value;
        }

        /// <summary>
        /// Read the value applying a compiler only fence, no CPU fence is applied
        /// </summary>
        /// <returns>The current value</returns>
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public bool ReadCompilerOnlyFence()
        {
            return ToBool(_value);
        }

        /// <summary>
        /// Write the value applying release fence semantic
        /// </summary>
        /// <param name="newValue">The new value</param>
        public void WriteReleaseFence(bool newValue)
        {
            int newValueInt = ToInt(newValue);
            Thread.MemoryBarrier();
            _value = newValueInt;
        }

        /// <summary>
        /// Write the value applying full fence semantic
        /// </summary>
        /// <param name="newValue">The new value</param>
        public void WriteFullFence(bool newValue)
        {
            int newValueInt = ToInt(newValue);
            Thread.MemoryBarrier();
            _value = newValueInt;
        }

        /// <summary>
        /// Write the value applying a compiler fence only, no CPU fence is applied
        /// </summary>
        /// <param name="newValue">The new value</param>
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public void WriteCompilerOnlyFence(bool newValue)
        {
            _value = ToInt(newValue);
        }

        /// <summary>
        /// Write without applying any fence
        /// </summary>
        /// <param name="newValue">The new value</param>
        public void WriteUnfenced(bool newValue)
        {
            _value = ToInt(newValue);
        }

        /// <summary>
        /// Atomically set the value to the given updated value if the current value equals the comparand
        /// </summary>
        /// <param name="newValue">The new value</param>
        /// <param name="comparand">The comparand (expected value)</param>
        /// <returns></returns>
        public bool CompareExchange(bool newValue, bool comparand)
        {
            int newValueInt = ToInt(newValue);
            int comparandInt = ToInt(comparand);
            return Interlocked.CompareExchange(ref _value, newValueInt, comparandInt) == comparandInt;
        }

        public void LazySet(bool value)
        {
            WriteCompilerOnlyFence(value);
        }

        /// <summary>
        /// Atomically set the value to the given updated value
        /// </summary>
        /// <param name="newValue">The new value</param>
        /// <returns>The original value</returns>
        public bool Exchange(bool newValue)
        {
            int newValueInt = ToInt(newValue);
            int originalValue = Interlocked.Exchange(ref _value, newValueInt);
            return ToBool(originalValue);
        }

        /// <summary>
        /// Returns the string representation of the current value.
        /// </summary>
        /// <returns>the string representation of the current value.</returns>
        public override string ToString()
        {
            bool value = ReadFullFence();
            return value.ToString();
        }

        public bool Value { get { return ReadFullFence(); } set { WriteFullFence(value); } }

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