using System.Threading.Tasks;
using NUnit.Framework;

namespace Fibrous.Tests;

[TestFixture]
public class FiberTests
{
    [Test]
    public void InOrderExecution()
    {
        FiberTester.InOrderExecution(new Fiber());
        FiberTester.InOrderExecution(new StubFiber());
        FiberTester.InOrderExecution(new LockFiber());
    }

    [Test]
    public void TestBatching()
    {
        FiberTester.TestBatching(new Fiber());
        FiberTester.TestBatching(new StubFiber());
        FiberTester.TestBatching(new LockFiber());
        FiberTester.TestBatchingWithKey(new Fiber());
        FiberTester.TestBatchingWithKey(new StubFiber());
        FiberTester.TestBatchingWithKey(new LockFiber());
        }

    [Test]
    public void TestPubSubSimple()
    {
        FiberTester.TestPubSubSimple(new Fiber());
        FiberTester.TestPubSubSimple(new StubFiber());
        FiberTester.TestPubSubSimple(new LockFiber());
    }

    [Test]
    public void TestPubSubWithFilter()
    {
        FiberTester.TestPubSubWithFilter(new Fiber());
        FiberTester.TestPubSubWithFilter(new StubFiber());
        FiberTester.TestPubSubWithFilter(new LockFiber());
    }

    [Test]
    public async Task TestReqReplyAsync()
    {
        await FiberTester.TestReqReplyAsync(new Fiber());
        await FiberTester.TestReqReplyAsync(new LockFiber());
        await FiberTester.TestReqReplyAsync(new StubFiber());
    }

    [Test]
    public void TestTwoFibers()
    {
        FiberTester.TestPubSubWExtraFiber(new Fiber(), new LockFiber());
        FiberTester.TestPubSubWExtraFiber(new LockFiber(), new LockFiber());
        FiberTester.TestPubSubWExtraFiber(new StubFiber(), new Fiber());
        FiberTester.TestPubSubWExtraFiber(new StubFiber(), new Fiber());
    }
}
