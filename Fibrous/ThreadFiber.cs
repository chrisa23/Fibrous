namespace Fibrous
{
    using System;
    using System.Threading;
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
        private volatile bool _running;

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
        /// <param name="executor"></param>
        /// <param name = "queue"></param>
        /// <param name = "threadName"></param>
        /// <param name = "isBackground"></param>
        /// <param name = "priority"></param>
        public ThreadFiber(Executor executor,
                           IFiberScheduler fiberScheduler,
                           IQueue queue,
                           string threadName,
                           bool isBackground = true,
                           ThreadPriority priority = ThreadPriority.Normal) : base(executor, fiberScheduler)
        {
            _queue = queue;
            _isBackground = isBackground;
            _priority = priority;
            _thread = new Thread(RunThread) { Name = threadName, IsBackground = _isBackground, Priority = _priority };
        }

        private static int GetNextThreadId()
        {
            return Interlocked.Increment(ref threadCount);
        }

        private void RunThread()
        {
            while (_running)
                _queue.Drain(Executor);
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
    }
}