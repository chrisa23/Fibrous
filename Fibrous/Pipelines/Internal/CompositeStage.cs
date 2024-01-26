using System;
using System.Threading.Tasks;

namespace Fibrous.Pipelines;

internal sealed class CompositeStage<TIn, TOut> : IStage<TIn, TOut>
{
    private readonly IDisposable _disposables;
    private readonly IPublisherPort<TIn> _input;
    private readonly ISubscriberPort<TOut> _output;

    public CompositeStage(IPublisherPort<TIn> input, ISubscriberPort<TOut> output, IDisposable disposables)
    {
        _input = input;
        _output = output;
        _disposables = disposables;
    }

    public void Publish(TIn msg) => _input.Publish(msg);


    public IDisposable Subscribe(IFiber fiber, Func<TOut, Task> receive) => _output.Subscribe(fiber, receive);

    public IDisposable Subscribe(Action<TOut> receive) => _output.Subscribe(receive);

    public void Dispose() => _disposables.Dispose();
}
