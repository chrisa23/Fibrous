namespace Fibrous.Pipeline
{
    using System;
    using Fibrous.Fibers;

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
            IPublisherPort<Exception> error,
            FiberType type = FiberType.Pool) : base(new ExceptionHandlingExecutor(error.Publish), type)
        {
            processor.Exception += error.Publish;
            processor.Output += output.Publish;
            processor.InitializeScheduling(this);
            input.Subscribe(Fiber, processor.Process);
        }
    }
}