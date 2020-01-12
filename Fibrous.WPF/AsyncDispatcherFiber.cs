using System;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Fibrous.WPF
{
    /// <summary>
    ///     Fiber for use with WPF forms and controls.Provides seamless marshalling to dispatcher thread.
    /// </summary>
    public sealed class AsyncDispatcherFiber : AsyncFiberBase
    {
        private readonly DispatcherPriority _priority;
        private readonly Dispatcher _dispatcher;

        public AsyncDispatcherFiber(IAsyncExecutor executor = null, Dispatcher dispatcher = null,
            DispatcherPriority priority = DispatcherPriority.Normal)
            : base(executor)
        {
            _dispatcher = dispatcher ?? Dispatcher.CurrentDispatcher;
            _priority = priority;
        }

        public AsyncDispatcherFiber(Action<Exception> errorCallback, Dispatcher dispatcher = null,
            DispatcherPriority priority = DispatcherPriority.Normal)
            : this(new AsyncExceptionHandlingExecutor(errorCallback), dispatcher, priority)
        {
        }

        protected override void InternalEnqueue(Func<Task> action)
        {
            _dispatcher.InvokeAsync(action, _priority);
        }
    }
}