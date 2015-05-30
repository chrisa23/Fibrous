namespace Fibrous
{
    using System;
    using System.Threading;

    /// <summary>
    ///   Fiber implementation backed by a dedicated thread., needs a thread safe queue
    ///   <see cref = "FiberBase" />
    /// </summary>
    public sealed class ThreadFiber : FiberBase
    {
        private static int threadCount;
        private readonly IQueue _queue;
        private readonly Thread _thread;
        private volatile bool _running;

        public ThreadFiber(string threadName)
            : this(new Executor(), new TimerScheduler(), new SleepingQueue(), threadName)
        {
        }

        public ThreadFiber(IExecutor executor, string name)
            : this(executor, new TimerScheduler(), new SleepingQueue(), name)
        {
        }

        public ThreadFiber(IExecutor executor)
            : this(executor, new TimerScheduler(), new SleepingQueue(), "ThreadFiber-" + GetNextThreadId())
        {
        }

        public ThreadFiber()
            : this("ThreadFiber-" + GetNextThreadId())
        {
        }

        /// <summary>
        ///   Creates a thread fiber.
        /// </summary>
        /// <param name="executor"></param>
        /// <param name = "queue"></param>
        /// <param name = "threadName"></param>
        /// <param name = "isBackground"></param>
        /// <param name = "priority"></param>
        public ThreadFiber(IExecutor executor,
            IFiberScheduler fiberScheduler,
            IQueue queue,
            string threadName,
            bool isBackground = true,
            ThreadPriority priority = ThreadPriority.Normal) : base(executor, fiberScheduler)
        {
            _queue = queue;
            _thread = new Thread(RunThread) { Name = threadName, IsBackground = isBackground, Priority = priority };
        }

        private static int GetNextThreadId()
        {
            return Interlocked.Increment(ref threadCount);
        }

        private void RunThread()
        {
            while (_running)
                Executor.Execute(_queue.Drain());
        }

        protected override void InternalEnqueue(Action action)
        {
            _queue.Enqueue(action);
        }

        protected override void InternalStart()
        {
            _running = true;
            _thread.Start();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _running = false;
                _queue.Dispose();
            }
            base.Dispose(disposing);
        }

        public static IFiber StartNew()
        {
            return Fiber.StartNew(FiberType.Thread);
        }

        public static IFiber StartNew(IExecutor executor)
        {
            return Fiber.StartNew(FiberType.Thread, executor);
        }

        public static IFiber StartNew(string name)
        {
            var fiber = new ThreadFiber(name);
            fiber.Start();
            return fiber;
        }
    }
}