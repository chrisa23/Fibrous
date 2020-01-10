using System;
using System.Threading.Tasks;

namespace Fibrous.Pipeline
{
    public abstract class StageBase<TIn, TOut> : IStage<TIn, TOut>
    {
        protected readonly IChannel<TIn> In = new Channel<TIn>();
        protected readonly IChannel<TOut> Out = new Channel<TOut>();

        protected StageBase(IExecutor executor = null)
        {
            Fiber = new Fiber(executor);
            Fiber.Subscribe(In, Receive);
        }

        public IFiber Fiber { get; }

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

        protected abstract void Receive(TIn @in);
    }
}