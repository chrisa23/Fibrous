using System.Threading.Tasks;
using NUnit.Framework;

namespace Fibrous.Tests;

[TestFixture]
public class FiberTests
{
    [Test]
    public void InOrderExecution()
    {
        FiberTester.InOrderExecution(new AsyncFiber());
        FiberTester.InOrderExecution(new AsyncStubFiber());
        FiberTester.InOrderExecution(new LockAsyncFiber());
    }

    [Test]
    public void TestBatching()
    {
        FiberTester.TestBatching(new AsyncFiber());
        FiberTester.TestBatching(new AsyncStubFiber());
        FiberTester.TestBatching(new LockAsyncFiber());
        FiberTester.TestBatchingWithKey(new AsyncFiber());
        FiberTester.TestBatchingWithKey(new AsyncStubFiber());
        FiberTester.TestBatchingWithKey(new LockAsyncFiber());
        }

    [Test]
    public void TestPubSubSimple()
    {
        FiberTester.TestPubSubSimple(new AsyncFiber());
        FiberTester.TestPubSubSimple(new AsyncStubFiber());
        FiberTester.TestPubSubSimple(new LockAsyncFiber());
    }

    [Test]
    public void TestPubSubWithFilter()
    {
        FiberTester.TestPubSubWithFilter(new AsyncFiber());
        FiberTester.TestPubSubWithFilter(new AsyncStubFiber());
        FiberTester.TestPubSubWithFilter(new LockAsyncFiber());
    }

    [Test]
    public async Task TestReqReplyAsync()
    {
        await FiberTester.TestReqReplyAsync(new AsyncFiber());
        await FiberTester.TestReqReplyAsync(new LockAsyncFiber());
        await FiberTester.TestReqReplyAsync(new AsyncStubFiber());
    }

    [Test]
    public void TestTwoFibers()
    {
        FiberTester.TestPubSubWExtraFiber(new AsyncFiber(), new LockAsyncFiber());
        FiberTester.TestPubSubWExtraFiber(new LockAsyncFiber(), new LockAsyncFiber());
        FiberTester.TestPubSubWExtraFiber(new AsyncStubFiber(), new AsyncFiber());
        FiberTester.TestPubSubWExtraFiber(new AsyncStubFiber(), new AsyncFiber());
    }
}
