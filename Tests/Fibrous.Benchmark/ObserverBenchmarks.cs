using System;
using System.Reactive.Subjects;
using System.Threading;
using BenchmarkDotNet.Attributes;
using Fibrous.Extras;

namespace Fibrous.Benchmark
{
    [MemoryDiagnoser]
    public class ObserverBenchmarks
    {
        private const int OperationsPerInvoke = 100000;


        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void Lock()
        {
            using AutoResetEvent reset = new AutoResetEvent(false);
            var observer = new TestObserver(reset, OperationsPerInvoke);
            var subject = new Subject<long>();
            using IDisposable dispose = subject.Subscribe(observer);

            for (int i = 0; i < OperationsPerInvoke; i++)
            {
                subject.OnNext(i + 1);
            }

            reset.WaitOne(TimeSpan.FromSeconds(10));
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void Wrapper()
        {
            using AutoResetEvent reset = new AutoResetEvent(false);
            using IFiber fiber = new Fiber();
            var observer = new WrappedObserver(reset, OperationsPerInvoke);
            var subject = new Subject<long>();
            using IDisposable dispose = subject.Subscribe(fiber, observer);

            for (int i = 0; i < OperationsPerInvoke; i++)
            {
                subject.OnNext(i + 1);
            }

            reset.WaitOne(TimeSpan.FromSeconds(10));
        }


        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void WrapperLock()
        {
            using AutoResetEvent reset = new AutoResetEvent(false);
            using IFiber fiber = new LockFiber();
            var observer = new WrappedObserver(reset, OperationsPerInvoke);
            var subject = new Subject<long>();
            using IDisposable dispose = subject.Subscribe(fiber, observer);

            for (int i = 0; i < OperationsPerInvoke; i++)
            {
                subject.OnNext(i + 1);
            }

            reset.WaitOne(TimeSpan.FromSeconds(10));
        }
    }


    public class TestObserver : AsyncObserver<long>
    {
        private readonly int _count;
        private readonly AutoResetEvent _evt;

        public TestObserver(AutoResetEvent evt, int count)
        {
            _evt = evt;
            _count = count;
        }

        protected override void Handle(long value)
        {
            if (value >= _count)
            {
                _evt.Set();
            }
        }

        protected override void HandleCompleted() { }

        protected override void HandleError(Exception exception)
        {
        }
    }


    public class WrappedObserver : IObserver<long>
    {
        private readonly int _count;
        private readonly AutoResetEvent _evt;

        public WrappedObserver(AutoResetEvent evt, int count)
        {
            _evt = evt;
            _count = count;
        }

        public void OnCompleted(){}

        public void OnError(Exception error)  { }

        public void OnNext(long value)
        {
            if (value >= _count)
            {
                _evt.Set();
            }
        }
    }
}
