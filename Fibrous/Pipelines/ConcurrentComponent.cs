using System;

namespace Fibrous.Pipeline
{
    /// <summary>
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    public sealed class ConcurrentComponent<TIn, TOut> : IDisposable
    {
        private readonly IFiber _fiber;

        public ConcurrentComponent(IProcessor<TIn, TOut> processor,
            ISubscriberPort<TIn> input,
            IPublisherPort<TOut> output,
            IPublisherPort<Exception> error)
        {
            _fiber = new Fiber(new ExceptionHandlingExecutor(error.Publish));
            processor.Exception += error.Publish;
            processor.Output += output.Publish;
            processor.Initialize(_fiber);
            input.Subscribe(_fiber, processor.Process);
        }

        public void Dispose()
        {
            _fiber?.Dispose();
        }
    }
}