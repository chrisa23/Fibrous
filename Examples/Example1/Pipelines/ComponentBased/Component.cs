﻿using System;
using Fibrous;

namespace Example1.Pipelines.ComponentBased;

public sealed class Component<TIn, TOut> : FiberComponent
{
    private readonly IPublisherPort<Exception> _error;
    private readonly IPublisherPort<TOut> _output;
    private readonly IAsyncProcessor<TIn, TOut> _processor;

    public Component(IAsyncProcessor<TIn, TOut> processor,
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
