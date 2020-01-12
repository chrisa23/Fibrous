using System;

namespace Fibrous.Pipelines
{
    public interface IStage<in TIn, out TOut> : IPublisherPort<TIn>, ISubscriberPort<TOut>, IDisposable
    {
      //  IFiber Fiber { get; }
    }

    //public interface IPipeline<in TIn, out TOut> : IPublisherPort<TIn>, ISubscriberPort<TOut>, IDisposable
    //{
     
    //}
}