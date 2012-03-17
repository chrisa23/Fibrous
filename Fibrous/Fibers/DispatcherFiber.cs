namespace Fibrous.Fibers
{
    using System;
    using System.Windows.Threading;

    public sealed class DispatcherFiber : GuiFiberBase
    {
        public DispatcherFiber(Dispatcher dispatcher, DispatcherPriority priority, IExecutor executor)
            : base(new DispatcherAdapter(dispatcher, priority), executor)
        {
        }

        public DispatcherFiber(Dispatcher dispatcher, IExecutor executor)
            : this(dispatcher, DispatcherPriority.Normal, executor)
        {
        }

        public DispatcherFiber(Dispatcher dispatcher, DispatcherPriority priority)
            : this(dispatcher, priority, new DefaultExecutor())
        {
        }

        public DispatcherFiber(Dispatcher dispatcher)
            : this(dispatcher, new DefaultExecutor())
        {
        }

        public DispatcherFiber(DispatcherPriority priority)
            : this(Dispatcher.CurrentDispatcher, priority, new DefaultExecutor())
        {
        }

        public DispatcherFiber()
            : this(Dispatcher.CurrentDispatcher, new DefaultExecutor())
        {
        }

        private class DispatcherAdapter : IExecutionContext
        {
            private readonly Dispatcher _dispatcher;
            private readonly DispatcherPriority _priority;

            public DispatcherAdapter(Dispatcher dispatcher, DispatcherPriority priority)
            {
                _dispatcher = dispatcher;
                _priority = priority;
            }

            public void Enqueue(Action action)
            {
                _dispatcher.BeginInvoke(action, _priority);
            }
        }
    }
}