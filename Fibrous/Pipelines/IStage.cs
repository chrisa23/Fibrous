using System;

namespace Fibrous.Pipelines
{
    public interface IStage<in TIn, out TOut> : IPublisherPort<TIn>, ISubscriberPort<TOut>, IDisposable
    {
        //void SubscribeTo( IStage<_,TIn> stage)
    }
}