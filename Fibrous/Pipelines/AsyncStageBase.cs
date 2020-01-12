using System;
using System.Threading.Tasks;

namespace Fibrous.Pipelines
{
    public abstract class AsyncStageBase<TIn, TOut> : IStage<TIn, TOut>, IHaveAsyncFiber
    {
        protected readonly IChannel<TIn> In = new Channel<TIn>();
        protected readonly IChannel<TOut> Out = new QueueChannel<TOut>();

        protected AsyncStageBase(Action<Exception> errorCallback = null)
        {
            IAsyncExecutor executor = errorCallback == null
                ? (IAsyncExecutor)new AsyncExecutor()
                : new AsyncExceptionHandlingExecutor(errorCallback);
            Fiber = new AsyncFiber(executor);
            Fiber.Subscribe(In, Receive);
        }

        public IAsyncFiber Fiber { get; }

        public void Publish(TIn msg)
        {
            In.Publish(msg);
        }

        public IDisposable Subscribe(IFiber fiber, Action<TOut> receive)
        {
            return Out.Subscribe(fiber, receive);
        }

        public IDisposable Subscribe(IAsyncFiber fiber, Func<TOut, Task> receive)
        {
            return Out.Subscribe(fiber, receive);
        }

        public void Dispose()
        {
            Fiber?.Dispose();
        }

        protected abstract Task Receive(TIn @in);
    }
}