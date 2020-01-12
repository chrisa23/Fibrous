using System;

namespace Fibrous.Pipelines
{
    /// <summary>
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    public sealed class Component<TIn, TOut> : IDisposable
    {
        private readonly IProcessor<TIn, TOut> _processor;
        private readonly IPublisherPort<TOut> _output;
        private readonly IPublisherPort<Exception> _error;
        private readonly IFiber _fiber;

        public Component(IProcessor<TIn, TOut> processor,
            ISubscriberPort<TIn> input,
            IPublisherPort<TOut> output,
            IPublisherPort<Exception> error)
        {
            _processor = processor;
            _output = output;
            _error = error;
            processor.Exception += error.Publish;
            processor.Output += output.Publish;
            _fiber = new Fiber(new ExceptionHandlingExecutor(error.Publish));
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