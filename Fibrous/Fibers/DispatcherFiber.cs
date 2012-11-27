namespace Fibrous.Fibers
{
    using System;
    using System.Windows.Threading;

    public sealed class DispatcherFiber : GuiFiberBase
    {
        public DispatcherFiber(FiberConfig config, Dispatcher dispatcher, DispatcherPriority priority)
            : base(config, new DispatcherAdapter(dispatcher, priority))
        {
        }

        public DispatcherFiber(Dispatcher dispatcher)
            : this(FiberConfig.Default, dispatcher, DispatcherPriority.Normal)
        {
        }

        public DispatcherFiber(Dispatcher dispatcher, DispatcherPriority priority)
            : this(FiberConfig.Default, dispatcher, priority)
        {
        }

        public DispatcherFiber(DispatcherPriority priority)
            : this(FiberConfig.Default, Dispatcher.CurrentDispatcher, priority)
        {
        }

        public DispatcherFiber()
            : this(FiberConfig.Default, Dispatcher.CurrentDispatcher, DispatcherPriority.Normal)
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