namespace Fibrous.Fibers
{
    using System;
    using System.Threading;
    using Fibrous.Fibers.Queues;

    /// <summary>
    ///   Fiber implementation backed by a dedicated thread.
    ///   <see cref = "IFiber" />
    /// </summary>
    public sealed class ThreadFiber : FiberBase
    {
        private static int threadCount;
        private readonly bool _isBackground;
        private readonly ThreadPriority _priority;
        private readonly IQueue _queue;
        private readonly Thread _thread;

        /// <summary>
        ///   Create a thread fiber with the default queue.
        /// </summary>
        public ThreadFiber()
            : this(new DefaultQueue())
        {
        }

        /// <summary>
        ///   Creates a thread fiber with a specified queue.
        /// </summary>
        /// <param name = "queue"></param>
        public ThreadFiber(IQueue queue)
            : this(queue, "ThreadFiber-" + GetNextThreadId())
        {
        }

        /// <summary>
        ///   Creates a thread fiber with a specified name.
        /// </summary>
        /// ///
        /// <param name = "threadName"></param>
        public ThreadFiber(string threadName)
            : this(new DefaultQueue(), threadName)
        {
        }

        /// <summary>
        ///   Creates a thread fiber.
        /// </summary>
        /// <param name = "queue"></param>
        /// <param name = "threadName"></param>
        /// <param name = "isBackground"></param>
        /// <param name = "priority"></param>
        public ThreadFiber(IQueue queue,
                           string threadName,
                           bool isBackground = true,
                           ThreadPriority priority = ThreadPriority.Normal)
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
            _queue.Run();
        }

        public override void Enqueue(Action action)
        {
            _queue.Enqueue(action);
        }

        public override void Start()
        {
            _thread.Start();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _queue.Stop();
            base.Dispose(disposing);
        }

        public static IFiber StartNew()
        {
            var fiber = new ThreadFiber();
            fiber.Start();
            return fiber;
        }

        public static IFiber StartNew(string name)
        {
            var fiber = new ThreadFiber(name);
            fiber.Start();
            return fiber;
        }
    }
}