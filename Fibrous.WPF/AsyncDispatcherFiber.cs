using System;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Fibrous.WPF
{
    /// <summary>
    ///     Fiber for use with WPF forms and controls.Provides seamless marshalling to dispatcher thread.
    /// </summary>
    public sealed class AsyncDispatcherFiber : AsyncGuiFiberBase
    {
        public AsyncDispatcherFiber(IAsyncExecutor executor, Dispatcher dispatcher,
            DispatcherPriority priority = DispatcherPriority.Normal)
            : base(executor, new AsyncDispatcherAdapter(dispatcher, priority))
        {
        }

        public AsyncDispatcherFiber(Dispatcher dispatcher, DispatcherPriority priority = DispatcherPriority.Normal)
            : this(new AsyncExecutor(), dispatcher, priority)
        {
        }

        public AsyncDispatcherFiber()
            : this(Dispatcher.CurrentDispatcher)
        {
        }

        public static IAsyncFiber StartNew() => new AsyncDispatcherFiber().Start();

        public static IAsyncFiber StartNew(IAsyncExecutor executor) => new AsyncDispatcherFiber(executor, Dispatcher.CurrentDispatcher).Start();

        private class AsyncDispatcherAdapter : IAsyncExecutionContext
        {
            private readonly Dispatcher _dispatcher;
            private readonly DispatcherPriority _priority;

            public AsyncDispatcherAdapter(Dispatcher dispatcher, DispatcherPriority priority)
            {
                _dispatcher = dispatcher;
                _priority = priority;
            }

            public void Enqueue(Func<Task> action)
            {
                _dispatcher.InvokeAsync(action, _priority);
            }
        }
    }
}