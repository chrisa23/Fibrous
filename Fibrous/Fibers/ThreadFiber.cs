namespace Fibrous.Fibers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Fibrous.Fibers.Queues;

    /// <summary>
    ///   Fiber implementation backed by a dedicated thread.
    ///   <see cref = "IFiber" />
    /// </summary>
    public sealed class ThreadFiber : FiberBase
    {
        //protected readonly object SyncRoot = new object();
        private static int threadCount;
        private readonly bool _isBackground;
        private readonly ThreadPriority _priority;
        //private readonly IWaitStrategy _wait;
        private readonly IQueue _queue;
        private readonly Thread _thread;
        private bool _running;
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
        public ThreadFiber(string threadName)
            : this(FiberConfig.Default, threadName)
        {
        }

        public ThreadFiber(IExecutor executor)
            : this(new FiberConfig(executor), "ThreadFiber-" + GetNextThreadId())
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
        public ThreadFiber(FiberConfig config,
                           //IWaitStrategy wait, 
                           string threadName,
                           bool isBackground = true,
                           ThreadPriority priority = ThreadPriority.Normal) : base(config)
        {
            _queue = new DefaultQueue();
            //  _wait = wait;
            _isBackground = isBackground;
            _priority = priority;
            _thread = new Thread(RunThread) { Name = threadName, IsBackground = _isBackground, Priority = _priority };
        }

        public ThreadFiber()
            : this("ThreadFiber-" + GetNextThreadId())
        {
        }

        public Thread Thread { get { return _thread; } }

        private static int GetNextThreadId()
        {
            return Interlocked.Increment(ref threadCount);
        }

        private void RunThread()
        {
            //run a loop waiting on queue...
            while (_running)
            {
                while (_queue.HasItems())
                    ExecuteNextBatch();
                //_wait.WaitForOtherThread();
            }
            //_queue.Run();
        }

        private bool ExecuteNextBatch()
        {
            IEnumerable<Action> toExecute = _queue.DequeueAll();
            if (toExecute == null)
                return false;
            Executor.Execute(toExecute);
            return true;
        }

        //private bool CheckBatch()
        //{
        //    long available = _queue.Available();
        //    for (int i = 0; i < available; i++)
        //    {
        //        Action action = _queue.Poll();
        //        if(action!= null)
        //            Executor.Execute(action);
        //    }
        //    return available > 0;
        //}
        //private void ExecuteBatch(Batch batch)
        //{
        //    for (int i = 0; i < batch.Count; i++)
        //    {
        //        Action execute = batch.Actions[i];
        //        if(execute!= null)
        //            Executor.Execute(execute);
        //    }
        //    batch.Clear();
        //}
        public override void Enqueue(Action action)
        {
            //  lock (SyncRoot)//switch to wait strategy
            //   {
            _queue.Enqueue(action);
            //      Monitor.PulseAll(SyncRoot);
            //}
            //  _wait.Signal();
        }

        public override void Start()
        {
            _running = true;
            _thread.Start();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _running = false;
                //       _wait.Signal();
            }
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
            fiber.Start();
            return fiber;
        }

        public static IFiber StartNew(FiberConfig config, string name)
        {
            var fiber = new ThreadFiber(config, name);
            fiber.Start();
            return fiber;
        }

        #endregion
    }
}