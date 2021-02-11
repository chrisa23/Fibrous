using System;
using System.Windows.Threading;

namespace Fibrous.WPF
{
    /// <summary>
    ///     Fiber for use with WPF forms and controls.Provides seamless marshalling to dispatcher thread.
    /// </summary>
    public sealed class DispatcherFiber : FiberBase
    {
        private readonly Dispatcher _dispatcher;
        private readonly DispatcherPriority _priority;

        public DispatcherFiber(IExecutor executor = null, Dispatcher dispatcher = null,
            DispatcherPriority priority = DispatcherPriority.Normal)
            : base(executor)
        {
            _dispatcher = dispatcher ?? Dispatcher.CurrentDispatcher;
            _priority = priority;
        }

        public DispatcherFiber(Action<Exception> errorCallback, Dispatcher dispatcher = null,
            DispatcherPriority priority = DispatcherPriority.Normal)
            : this(new ExceptionHandlingExecutor(errorCallback), dispatcher, priority)
        {
        }

        protected override void InternalEnqueue(Action action) => _dispatcher.BeginInvoke(action, _priority);
    }
}
