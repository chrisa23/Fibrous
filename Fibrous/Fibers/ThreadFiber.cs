namespace Fibrous.Fibers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Fibrous.Fibers.Queues;
    using Fibrous.Scheduling;

    /// <summary>
    ///   Fiber implementation backed by a dedicated thread., needs a thread safe queue
    ///   <see cref = "IFiber" />
    /// </summary>
    public sealed class ThreadFiber : FiberBase
    {
        private static int threadCount;
        private readonly bool _isBackground;
        private readonly ThreadPriority _priority;
        private readonly IQueue _queue;
        private readonly Thread _thread;
        private PaddedBoolean _running = new PaddedBoolean(false);
        ///// <summary>
        /////   Creates a thread fiber with a specified queue.
        ///// </summary>
        ///// <param name = "wait"></param>
        //public ThreadFiber(IWaitStrategy wait)
        //    : this(FiberConfig.Default, wait, "ThreadFiber-" + GetNextThreadId())
        //{
        //}
        /// <summary>
        ///   Creates a thread fiber with a specified name.
        /// </summary>
        /// ///
        /// <param name = "threadName"></param>
        public ThreadFiber(string threadName) : this(new DefaultExecutor(), new TimerScheduler(), threadName)
        {
        }

        public ThreadFiber(IExecutor executor, string name)
            : this(executor, new TimerScheduler(), name)
        {
        }

        public ThreadFiber(IExecutor executor)
            : this(executor, new TimerScheduler(), "ThreadFiber-" + GetNextThreadId())
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
        public ThreadFiber(IExecutor config,
                           IFiberScheduler fiberScheduler,
                           string threadName,
                           bool isBackground = true,
                           ThreadPriority priority = ThreadPriority.Normal) : base(config, fiberScheduler)
        {
            _queue =  new DefaultQueue(); // new DisruptorQueue(1024*1024);
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
            while (_running.ReadUnfenced())
                ExecuteNextBatch();
        }

        private void ExecuteNextBatch()
        {
            //IEnumerable<Action> toExecute = _queue.DequeueAll();
            //Executor.Execute(toExecute);
            _queue.Drain(Executor);
        }

        public override void Enqueue(Action action)
        {
            _queue.Enqueue(action);
        }

        public override IFiber Start()
        {
            _running.Exchange(true);
            _thread.Start();
            //Enqueue(() => {});
            return this;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _running.LazySet(false);
            base.Dispose(disposing);
        }

        #region StartNew

        public static IFiber StartNew()
        {
            var fiber = new ThreadFiber();
            fiber.Start();
            return fiber;
        }

        public static IFiber StartNew(string name)
        {
            var fiber = new ThreadFiber(name);
            return fiber.Start();
        }

        public static IFiber StartNew(IExecutor config, string name)
        {
            var fiber = new ThreadFiber(config, name);
            return fiber.Start();
        }

        #endregion
    }
}