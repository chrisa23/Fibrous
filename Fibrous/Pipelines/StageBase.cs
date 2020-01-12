using System;
using System.Threading.Tasks;

namespace Fibrous.Pipelines
{
    public abstract class StageBase<TIn, TOut> : IStage<TIn, TOut>, IHaveFiber
    {
        protected readonly IChannel<TIn> In = new Channel<TIn>();
        protected readonly IChannel<TOut> Out = new QueueChannel<TOut>();

        protected StageBase(Action<Exception> errorCallback = null)
        {
            IExecutor executor = errorCallback == null
                ? (IExecutor)new Executor()
                : new ExceptionHandlingExecutor(errorCallback);
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

    public class CompositeStage<TIn, TOut> : IStage<TIn, TOut>, IHaveFiber
    {
        private readonly IPublisherPort<TIn> _input;
        private readonly ISubscriberPort<TOut> _output;
        private readonly IDisposable _disposables;

        public CompositeStage(IPublisherPort<TIn> input, ISubscriberPort<TOut> output, IFiber fiber, IDisposable disposables)
        {
            _input = input;
            _output = output;
            _disposables = disposables;
            Fiber = fiber;
        }

        public void Publish(TIn msg)
        {
            _input.Publish(msg);
        }

        public IDisposable Subscribe(IFiber fiber, Action<TOut> receive)
        {
            return _output.Subscribe(fiber, receive);
        }

        public IDisposable Subscribe(IAsyncFiber fiber, Func<TOut, Task> receive)
        {
            return _output.Subscribe(fiber, receive);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        public IFiber Fiber { get; }
    }

    public class CompositeAsyncStage<TIn, TOut> : IStage<TIn, TOut>, IHaveAsyncFiber
    {
        private readonly IPublisherPort<TIn> _input;
        private readonly ISubscriberPort<TOut> _output;
        private readonly IDisposable _disposables;

        public CompositeAsyncStage(IPublisherPort<TIn> input, ISubscriberPort<TOut> output, IAsyncFiber fiber, IDisposable disposables)
        {
            _input = input;
            _output = output;
            _disposables = disposables;
            Fiber = fiber;
        }

        public void Publish(TIn msg)
        {
            _input.Publish(msg);
        }

        public IDisposable Subscribe(IFiber fiber, Action<TOut> receive)
        {
            return _output.Subscribe(fiber, receive);
        }

        public IDisposable Subscribe(IAsyncFiber fiber, Func<TOut, Task> receive)
        {
            return _output.Subscribe(fiber, receive);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        public IAsyncFiber Fiber { get; }
    }
}