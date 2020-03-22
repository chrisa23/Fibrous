using NUnit.Framework;

namespace Fibrous.Tests
{
    [TestFixture]
    public class FiberTests
    {
        [Test]
        public void InOrderExecution()
        {
            FiberTester.InOrderExecution(new AsyncFiber());
            FiberTester.InOrderExecution(new AsyncStubFiber());
            FiberTester.InOrderExecution(new Fiber());
            FiberTester.InOrderExecution(new StubFiber());
        }

        [Test]
        public void TestBatching()
        {
            FiberTester.TestBatching(new AsyncFiber());
            FiberTester.TestBatching(new AsyncStubFiber());
            FiberTester.TestBatching(new Fiber());
            FiberTester.TestBatching(new StubFiber());
            FiberTester.TestBatchingWithKey(new AsyncFiber());
            FiberTester.TestBatchingWithKey(new AsyncStubFiber());
            FiberTester.TestBatchingWithKey(new Fiber());
            FiberTester.TestBatchingWithKey(new StubFiber());
        }

        [Test]
        public void TestPubSubSimple()
        {
            FiberTester.TestPubSubSimple(new AsyncFiber());
            FiberTester.TestPubSubSimple(new AsyncStubFiber());
            FiberTester.TestPubSubSimple(new Fiber());
            FiberTester.TestPubSubSimple(new StubFiber());
        }

        [Test]
        public void TestPubSubWithFilter()
        {
            FiberTester.TestPubSubWithFilter(new AsyncFiber());
            FiberTester.TestPubSubWithFilter(new AsyncStubFiber());
            FiberTester.TestPubSubWithFilter(new Fiber());
            FiberTester.TestPubSubWithFilter(new StubFiber());
        }

        [Test]
        public void TestReqReply()
        {
            FiberTester.TestReqReply1(new Fiber());
            FiberTester.TestReqReply1(new StubFiber());
        }

        [Test]
        public void TestTwoFibers()
        {
            FiberTester.TestPubSubWExtraFiber(new AsyncFiber(), new Fiber());
            FiberTester.TestPubSubWExtraFiber(new Fiber(), new Fiber());
            FiberTester.TestPubSubWExtraFiber(new Fiber(), new StubFiber());
        }
    }
}