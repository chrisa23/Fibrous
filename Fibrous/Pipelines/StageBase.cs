using System.Threading.Tasks;

namespace Fibrous.Pipeline
{
    using System;

    public abstract class StageBase<TIn, TOut> : IStage<TIn, TOut>
    {
        protected readonly IChannel<TIn> In = new Channel<TIn>();
        protected readonly IChannel<TOut> Out = new Channel<TOut>();
        public IFiber Fiber { get; }

        protected StageBase( IExecutor executor = null)
        {
            Fiber = PoolFiber.StartNew(executor);
            Fiber.Subscribe(In, Receive);
        }

        protected abstract void Receive(TIn @in);

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
    }
}