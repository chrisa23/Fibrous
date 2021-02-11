using System;

namespace Fibrous
{
    public static class ObservableExtensions
    {
        private sealed class ObserverWrapper<T> : IObserver<T>
        {
            private readonly IFiber _fiber;
            private readonly IObserver<T> _toWrap;

            public ObserverWrapper(IFiber fiber, IObserver<T> toWrap)
            {
                _fiber = fiber;
                _toWrap = toWrap;
            }

            public void OnCompleted() => _fiber.Enqueue(_toWrap.OnCompleted);

            public void OnError(Exception error) => _fiber.Enqueue(() => _toWrap.OnError(error));

            public void OnNext(T value) => _fiber.Enqueue(() => _toWrap.OnNext(value));

        }

        public static IDisposable Subscribe<T>(this IObservable<T> observable, IFiber fiber, IObserver<T> observer)
        {
            return observable.Subscribe(new ObserverWrapper<T>(fiber, observer));
        }
    }
}
