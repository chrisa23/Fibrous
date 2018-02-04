namespace Fibrous.Tests
{
    using System.Threading;
    using Fibrous.DSL;
    using Fibrous.Fibers;
    using NUnit.Framework;

    [TestFixture]
    public class FiberBuilderTests
    {
        [Test]
        public void BuilderTests()
        {
            IFiber threadFiber =
                FiberBuilder
                    .Create(FiberType.Thread)
                    .WithBlockingQueue(1000)
                    .WithErrorHandlingExecutor()
                    .WithPriority(ThreadPriority.Highest)
                    .Start();
            IFiber pool =
                FiberBuilder
                    .Create(FiberType.Pool)
                    .Start();
        }
    }
}