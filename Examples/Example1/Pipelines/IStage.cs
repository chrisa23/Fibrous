using System;
using Fibrous;

namespace Example1.Pipelines;

public interface IStage<in TIn, out TOut> : IPublisherPort<TIn>, ISubscriberPort<TOut>, IDisposable
{
}
