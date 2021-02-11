using System;
using System.Threading.Tasks;
using Fibrous.Proxy;
using NUnit.Framework;

namespace Fibrous.Tests
{
    [TestFixture]
    public class ProxyTests
    {
        [Test]
        public async Task ProxyWorks()
        {
            Test t = new Test();
            ITest tp = FiberProxy<ITest>.Create(t);
            tp.Init();
            tp.Add(1);
            tp.Subtract(2);
            await Task.Delay(100);

            Assert.IsTrue(t.Inited);
            Assert.AreEqual(-1, t.Count);
            int result = 0;
            tp.Event1 += x => result = x;

            t.Trigger();
            await Task.Delay(100);

            Assert.AreEqual(1, result);

            tp.Dispose();
            Assert.IsTrue(t.Inited);
            Assert.AreEqual(-1, t.Count);
            Assert.IsTrue(t.Disposed);
        }

        [Test]
        public async Task AsyncProxyWorks()
        {
            AsyncTest t = new AsyncTest();
            IAsyncTest tp = AsyncFiberProxy<IAsyncTest>.Create(t);
            await tp.Init();
            await tp.Add(1);
            await tp.Subtract(2);
            await Task.Delay(100);

            Assert.IsTrue(t.Inited);
            Assert.AreEqual(-1, t.Count);
            int result = 0;
            tp.Event1 += x => result = x;

            t.Trigger();
            await Task.Delay(100);

            Assert.AreEqual(1, result);

            tp.Dispose();
            Assert.IsTrue(t.Inited);
            Assert.AreEqual(-1, t.Count);
            Assert.IsTrue(t.Disposed);
        }
    }

    public interface ITest : IDisposable
    {
        void Init();
        void Add(int i);
        void Subtract(int i);

        event Action<int> Event1;
        // int DO();
    }

    public interface IAsyncTest : IDisposable
    {
        Task Init();
        Task Add(int i);
        Task Subtract(int i);

        event Action<int> Event1;
        // int DO();
    }

    public class Test : ITest
    {
        public bool Inited { get; set; }
        public bool Disposed { get; set; }
        public int Count { get; set; }

        public void Init() => Inited = true;

        public void Add(int i) => Count += i;

        public void Subtract(int i) => Count -= i;

        public event Action<int> Event1;

        public void Dispose() => Disposed = true;

        public int DO() => throw new NotImplementedException();

        public void Trigger() => Event1?.Invoke(1);
    }

    public class AsyncTest : IAsyncTest
    {
        public bool Inited { get; set; }
        public bool Disposed { get; set; }
        public int Count { get; set; }

        public Task Init()
        {
            Inited = true;
            return Task.CompletedTask;
        }

        public Task Add(int i)
        {
            Count += i;
            return Task.CompletedTask;
        }

        public Task Subtract(int i)
        {
            Count -= i;
            return Task.CompletedTask;
        }

        public event Action<int> Event1;

        public void Dispose() => Disposed = true;

        public int DO() => throw new NotImplementedException();

        public void Trigger() => Event1?.Invoke(1);
    }
}
