namespace Fibrous.Pipeline
{
    using System;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    public sealed class ConcurrentComponent<TIn, TOut> : ConcurrentComponentBase
    {
        public ConcurrentComponent(IProcessor<TIn, TOut> processor,
            ISubscriberPort<TIn> input,
            IPublisherPort<TOut> output,
            IPublisherPort<Exception> error) : base(new ExceptionHandlingExecutor(error.Publish))
        {
            processor.Exception += error.Publish;
            processor.Output += output.Publish;
            processor.Initialize(this);
            input.Subscribe(Fiber, processor.Process);
        }
    }
}