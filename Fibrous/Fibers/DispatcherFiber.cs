namespace Fibrous
{
    using System;
    using System.Windows.Threading;

    /// <summary>
    /// Fiber for use with WPF forms and controls.  Provides seamless marshalling to dispatcher thread.
    /// </summary>
    public sealed class DispatcherFiber : GuiFiberBase
    {
        public DispatcherFiber(Executor executor, Dispatcher dispatcher, DispatcherPriority priority = DispatcherPriority.Normal)
            : base(executor, new DispatcherAdapter(dispatcher, priority))
        {
        }

        public DispatcherFiber(Dispatcher dispatcher, DispatcherPriority priority = DispatcherPriority.Normal)
            : this(new Executor(), dispatcher, priority)
        {
        }

        public DispatcherFiber()
            : this(Dispatcher.CurrentDispatcher)
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