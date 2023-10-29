using System;
using System.Collections.Generic;
using System.Threading;

namespace Fibrous
{
    /// <summary>
    ///     Fiber implementation backed by a dedicated thread., needs a thread safe queue
    ///     <see cref="FiberBase" />
    /// </summary>
    public sealed class ThreadFiber : FiberBase_old
    {
        private static int threadCount;
        private readonly IQueue _queue;
        private readonly Thread _thread;
        private volatile bool _running;

        public ThreadFiber(string threadName)
            : this(new Executor(), new TimerScheduler(), new YieldingQueue(), threadName)
        {
        }

        public ThreadFiber(IExecutor executor, string name)
            : this(executor, new TimerScheduler(), new YieldingQueue(), name)
        {
        }

        public ThreadFiber(IExecutor executor)
            : this(executor, new TimerScheduler(), new YieldingQueue(), "ThreadFiber-" + GetNextThreadId())
        {
        }

        public ThreadFiber(IExecutor executor, IQueue queue)
            : this(executor, new TimerScheduler(), queue, "ThreadFiber-" + GetNextThreadId())
        {
        }

        public ThreadFiber()
            : this("ThreadFiber-" + GetNextThreadId())
        {
        }

        /// <summary>
        ///     Creates a thread fiber.
        /// </summary>
        /// <param name="executor"></param>
        /// <param name="fiberScheduler"></param>
        /// <param name="queue"></param>
        /// <param name="threadName"></param>
        /// <param name="isBackground"></param>
        /// <param name="priority"></param>
        public ThreadFiber(IExecutor executor,
            IFiberScheduler fiberScheduler,
            IQueue queue,
            string threadName,
            bool isBackground = true,
            ThreadPriority priority = ThreadPriority.Normal) : base(executor, fiberScheduler)
        {
            if (threadName == null)
            {
                threadName = "ThreadFiber-" + GetNextThreadId();
            }

            _queue = queue;
            _thread = new Thread(RunThread) {Name = threadName, IsBackground = isBackground, Priority = priority};
        }

        private static int GetNextThreadId() => Interlocked.Increment(ref threadCount);

        private void RunThread()
        {
            while (_running)
            {
                List<Action> list = _queue.Drain();
                for (int i = 0; i < list.Count; i++)
                {
                    Executor.Execute(list[i]);
                }
            }
        }

        protected override void InternalEnqueue(Action action) => _queue.Enqueue(action);

        protected override void InternalStart()
        {
            _running = true;
            _thread.Start();
        }

        public override void Dispose()
        {
            _running = false;
            _queue.Dispose();

            base.Dispose();
        }

        public static IFiber StartNew()
        {
            ThreadFiber pool = new();
            pool.Start();
            return pool;
        }

        public static IFiber StartNew(IExecutor executor)
        {
            ThreadFiber pool = new(executor);
            pool.Start();
            return pool;
        }

        public static IFiber StartNew(string name)
        {
            ThreadFiber fiber = new(name);
            fiber.Start();
            return fiber;
        }
    }
}
