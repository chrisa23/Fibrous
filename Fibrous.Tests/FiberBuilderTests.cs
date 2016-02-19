using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fibrous.Tests
{
    using System.Threading;
    using Fibrous;
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
