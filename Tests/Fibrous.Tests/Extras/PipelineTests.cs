using System.Threading;
using Fibrous.Pipelines;
using NUnit.Framework;

namespace Fibrous.Tests
{
    [TestFixture]
    public class PipelineTests
    {
        [Test]
        public void PipelineTest1()
        {
            string value = null;
            var tvalue = 0;
            var pipeline = new Stage<double, int>(d => (int) d)
                .Tap(i => tvalue = i)
                .Select(i => (i * 10).ToString());
            var stub = new StubFiber();
            pipeline.Subscribe(stub, s => value = s);
            pipeline.Publish(1.3);
            Thread.Sleep(100);
            Assert.AreEqual("10", value);
            Assert.AreEqual(1, tvalue);
        }
    }
}