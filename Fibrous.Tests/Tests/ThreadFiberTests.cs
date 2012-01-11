using NUnit.Framework;
using Retlang.Core;
using Retlang.Fibers;

namespace RetlangTests.Tests
{
    [TestFixture]
    public class ThreadFiberTests
    {
        [Test]
        public void RunThread()
        {
            ThreadFiber threadFiber = new ThreadFiber(new CommandQueue());
            threadFiber.Start();
            threadFiber.Dispose();
            threadFiber.Join();
        }

        [Test]
        public void AsyncStop()
        {
            ThreadFiber threadFiber = new ThreadFiber(new CommandQueue());
            threadFiber.Start();
            threadFiber.Enqueue(threadFiber.Dispose);
            threadFiber.Join();
        }
    }
}