using System.Threading;
using Fibrous.Pipelines;
using NUnit.Framework;

namespace Fibrous.Tests;

[TestFixture]
public class PipelineTests
{
    [Test]
    public void PipelineTest1()
    {
        string value = null;
        int tvalue = 0;
        IStage<double, string> pipeline = new AsyncStage<double, int>(async d => (int)d)
            .Tap(i => tvalue = i)
            .Select(async i => (i * 10).ToString());
        StubFiber stub = new();
        pipeline.Subscribe(stub, async s => value = s);
        pipeline.Publish(1.3);
        Thread.Sleep(100);
        Assert.AreEqual("10", value);
        Assert.AreEqual(1, tvalue);
    }
}
