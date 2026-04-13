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
    }

    [Test]
    public void TestBatching()
    {
        FiberTester.TestBatching(new Fiber());
        FiberTester.TestBatching(new StubFiber());
        FiberTester.TestBatchingWithKey(new Fiber());
        FiberTester.TestBatchingWithKey(new StubFiber());
        }

    [Test]
    public void TestPubSubSimple()
    {
        FiberTester.TestPubSubSimple(new Fiber());
        FiberTester.TestPubSubSimple(new StubFiber());
    }

    [Test]
    public void TestPubSubWithFilter()
    {
        FiberTester.TestPubSubWithFilter(new Fiber());
        FiberTester.TestPubSubWithFilter(new StubFiber());
    }

    [Test]
    public async Task TestReqReplyAsync()
    {
        await FiberTester.TestReqReplyAsync(new Fiber());
        await FiberTester.TestReqReplyAsync(new StubFiber());
    }

    [Test]
    public void TestTwoFibers()
    {
        FiberTester.TestPubSubWExtraFiber(new Fiber(), new Fiber());
        FiberTester.TestPubSubWExtraFiber(new Fiber(), new StubFiber());
        FiberTester.TestPubSubWExtraFiber(new StubFiber(), new Fiber());
        FiberTester.TestPubSubWExtraFiber(new StubFiber(), new Fiber());
    }
}
