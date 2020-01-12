using System;

namespace Fibrous.Pipelines
{
    public sealed class AsyncComponent<TIn, TOut> : IDisposable
    {
        private readonly IAsyncProcessor<TIn, TOut> _processor;
        private readonly IPublisherPort<TOut> _output;
        private readonly IPublisherPort<Exception> _error;
        private readonly IAsyncFiber _fiber;

        public AsyncComponent(IAsyncProcessor<TIn, TOut> processor,
            ISubscriberPort<TIn> input,
            IPublisherPort<TOut> output,
            IPublisherPort<Exception> error)
        {
            _processor = processor;
            _output = output;
            _error = error;
            processor.Exception += error.Publish;
            processor.Output += output.Publish;
            _fiber = new AsyncFiber(new AsyncExceptionHandlingExecutor(error.Publish));
            processor.Initialize(_fiber);
            input.Subscribe(_fiber, processor.Process);
        }

        public void Dispose()
        {
            _processor.Exception -= _error.Publish;
            _processor.Output -= _output.Publish;
            _fiber?.Dispose();
        }
    }
}