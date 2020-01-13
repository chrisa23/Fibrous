using System;
using System.Threading.Tasks;

namespace Fibrous.Pipelines
{
    internal sealed class CompositeStage<TIn, TOut> : IStage<TIn, TOut>, IHaveFiber
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
}