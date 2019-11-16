using System;

namespace Fibrous.Pipeline
{
    public interface IStage<in TIn, out TOut> : IPublisherPort<TIn>, ISubscriberPort<TOut>, IDisposable
    {
        IFiber Fiber { get; }
    }
}