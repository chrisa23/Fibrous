/*
using System;

namespace Fibrous;

public static class ObservableExtensions
{
    public static IDisposable Subscribe<T>(this IObservable<T> observable, IFiber fiber, IObserver<T> observer) =>
        observable.Subscribe(new ObserverWrapper<T>(fiber, observer));

    public static IDisposable Subscribe<T>(this IObservable<T> observable, IFiber fiber, Action<T> handler) =>
        observable.Subscribe(new FiberObserver<T>(fiber, handler));

    private sealed class ObserverWrapper<T> : IObserver<T>
    {
        private readonly IFiber _fiber;
        private readonly IObserver<T> _toWrap;

        public ObserverWrapper(IFiber fiber, IObserver<T> toWrap)
        {
            _fiber = fiber;
            _toWrap = toWrap;
        }

        public void OnNext(T value) => _fiber.Enqueue(() => _toWrap.OnNext(value));
        public void OnError(Exception error) => _fiber.Enqueue(() => _toWrap.OnError(error));
        public void OnCompleted() => _fiber.Enqueue(_toWrap.OnCompleted);
    }

    private sealed class FiberObserver<T> : IObserver<T>
    {
        private readonly IFiber _fiber;
        private readonly Action<T> _toWrap;

        public FiberObserver(IFiber fiber, Action<T> toWrap)
        {
            _fiber = fiber;
            _toWrap = toWrap;
        }

        public void OnNext(T value) => _fiber.Enqueue(() => _toWrap(value));

        public void OnError(Exception error)
        {
        }

        public void OnCompleted()
        {
        }
    }
}
*/
