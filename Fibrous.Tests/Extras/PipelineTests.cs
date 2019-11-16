namespace Fibrous.Tests
{
    using System.Threading;
    using Fibrous.Pipeline;
    using NUnit.Framework;

    [TestFixture]
    public class PipelineTests
    {
        [Test]
        public void PipelineTest1()
        {
            string value = null;
            int tvalue = 0;
            var stage1 = new Stage<double, int>(d => (int)d);
            var stage2 = new Tee<int>(i => tvalue = i);
            var stage3 = new Stage<int, string>(i => (i * 10).ToString());
            var stub = StubFiber.StartNew();
            var pipeline = stage1.To(stage2).To(stage3);
            pipeline.Subscribe(stub, s => value = s);
            stage1.Publish(1.3);
            Thread.Sleep(10);
            Assert.AreEqual("10", value);
            Assert.AreEqual(1, tvalue);
        }
    }
}