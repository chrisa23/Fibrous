using System;

namespace Fibrous;

public static class ObservableExtensions
{
    public static IDisposable Subscribe<T>(this IObservable<T> observable, IFiber fiber, IObserver<T> observer) =>
        observable.Subscribe(new ObserverWrapper<T>(fiber, observer));

    public static IDisposable Subscribe<T>(this IObservable<T> observable, IFiber fiber, Action<T> handler) =>
        observable.Subscribe(new FiberObserver<T>(fiber, handler));

    private sealed class ObserverWrapper<T>(IFiber fiber, IObserver<T> toWrap) : IObserver<T>
    {
        public void OnNext(T value) => fiber.Enqueue(() => toWrap.OnNext(value));
        public void OnError(Exception error) => fiber.Enqueue(() => toWrap.OnError(error));
        public void OnCompleted() => fiber.Enqueue(toWrap.OnCompleted);
    }

    private sealed class FiberObserver<T>(IFiber fiber, Action<T> toWrap) : IObserver<T>
    {
        public void OnNext(T value) => fiber.Enqueue(() => toWrap(value));

        public void OnError(Exception error)
        {
        }

        public void OnCompleted()
        {
        }
    }
}
