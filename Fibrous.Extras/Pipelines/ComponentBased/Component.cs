using System;

namespace Fibrous.Pipelines
{
    /// <summary>
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    public sealed class Component<TIn, TOut> : FiberComponent
    {
        private readonly IProcessor<TIn, TOut> _processor;
        private readonly IPublisherPort<TOut> _output;
        private readonly IPublisherPort<Exception> _error;

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
            processor.Initialize(Fiber);
            input.Subscribe(Fiber, processor.Process);
        }
        protected override void OnError(Exception obj) => _error.Publish(obj);

        public new void Dispose()
        {
            _processor.Exception -= _error.Publish;
            _processor.Output -= _output.Publish;
            base.Dispose();
        }
    }
}