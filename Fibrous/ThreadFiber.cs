namespace Fibrous
{
    using System;
    using System.Threading;
    using Fibrous.Experimental;
    using Fibrous.Queues;
    using Fibrous.Scheduling;

    /// <summary>
    ///   Fiber implementation backed by a dedicated thread., needs a thread safe queue
    ///   <see cref = "Fiber" />
    /// </summary>
    public sealed class ThreadFiber : Fiber
    {
        private static int threadCount;
        private readonly bool _isBackground;
        private readonly ThreadPriority _priority;
        private readonly IQueue _queue;
        private readonly Thread _thread;
        private PaddedBoolean _running = new PaddedBoolean(false);

        /// <summary>
        ///   Creates a thread fiber with a specified name.
        /// </summary>
        /// ///
        /// <param name = "threadName"></param>
        public ThreadFiber(string threadName)
            : this(new Executor(), new TimerScheduler(), new DefaultQueue(), threadName)
        {
        }

        public ThreadFiber(Executor executor, string name)
            : this(executor, new TimerScheduler(), new DefaultQueue(), name)
        {
        }

        public ThreadFiber(Executor executor)
            : this(executor, new TimerScheduler(), new DefaultQueue(), "ThreadFiber-" + GetNextThreadId())
        {
        }

        public ThreadFiber()
            : this("ThreadFiber-" + GetNextThreadId())
        {
        }

        /// <summary>
        ///   Creates a thread fiber.
        /// </summary>
        /// <param name="config"></param>
        /// <param name = "queue"></param>
        /// <param name = "threadName"></param>
        /// <param name = "isBackground"></param>
        /// <param name = "priority"></param>
        public ThreadFiber(Executor config,
                           IFiberScheduler fiberScheduler,
                           IQueue queue,
                           string threadName,
                           bool isBackground = true,
                           ThreadPriority priority = ThreadPriority.Normal) : base(config, fiberScheduler)
        {
            _queue = queue;
            _isBackground = isBackground;
            _priority = priority;
            _thread = new Thread(RunThread) { Name = threadName, IsBackground = _isBackground, Priority = _priority };
        }

        public Thread Thread { get { return _thread; } }

        private static int GetNextThreadId()
        {
            return Interlocked.Increment(ref threadCount);
        }

        private void RunThread()
        {
            while (_running.Value)
                ExecuteNextBatch();
        }

        private void ExecuteNextBatch()
        {
            _queue.Drain(Executor);
        }

        protected override void InternalEnqueue(Action action)
        {
            _queue.Enqueue(action);
        }

        protected override void InternalStart()
        {
            _running.Exchange(true);
            _thread.Start();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _running.LazySet(false);
            base.Dispose(disposing);
        }

        #region StartNew

        public static Fiber StartNew()
        {
            var fiber = new ThreadFiber();
            fiber.Start();
            return fiber;
        }

        public static Fiber StartNew(string name)
        {
            var fiber = new ThreadFiber(name);
            return fiber.Start();
        }

        public static Fiber StartNew(Executor config, string name)
        {
            var fiber = new ThreadFiber(config, name);
            return fiber.Start();
        }

        #endregion
    }
}