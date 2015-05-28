namespace Fibrous
{
    using System;
    using System.ComponentModel;
    using System.Windows.Threading;

    public abstract class GuiFiber : FiberBase
    {
        private readonly IExecutionContext _executionContext;

        protected GuiFiber(Executor executor, IExecutionContext executionContext) : base(executor)
        {
            _executionContext = executionContext;
        }

        protected GuiFiber(IExecutionContext executionContext)
        {
            _executionContext = executionContext;
        }

        protected override void InternalEnqueue(Action action)
        {
            _executionContext.Enqueue(() => Executor.Execute(action));
        }
    }

    /// <summary>
    /// Fiber for use with WPF forms and controls.  Provides seamless marshalling to dispatcher thread.
    /// </summary>
    public sealed class DispatcherFiber : GuiFiber
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


    public sealed class FormFiber : GuiFiber
    {
        public FormFiber(Executor executor, ISynchronizeInvoke invoker)
            : base(executor, new FormAdapter(invoker))
        {
        }

        public FormFiber(ISynchronizeInvoke invoker)
            : base(new FormAdapter(invoker))
        {
        }

        public static FiberBase StartNew(ISynchronizeInvoke invoker)
        {
            var fiber = new FormFiber(invoker);
            fiber.Start();
            return fiber;
        }

        public static FiberBase StartNew(Executor executor, ISynchronizeInvoke invoker)
        {
            var fiber = new FormFiber(executor, invoker);
            fiber.Start();
            return fiber;
        }

        private class FormAdapter : IExecutionContext
        {
            private readonly ISynchronizeInvoke _invoker;

            public FormAdapter(ISynchronizeInvoke invoker)
            {
                _invoker = invoker;
            }

            public void Enqueue(Action action)
            {
                _invoker.BeginInvoke(action, null);
            }
        }
    }
}