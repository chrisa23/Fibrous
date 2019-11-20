using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Fibrous.Tests
{
    [TestFixture]
    public class ExceptionHandlingTests
    {
        [Test]
        public void ExceptionHandlingExecutor()
        {
            using var reset = new AutoResetEvent(false);
            var h = new ExceptionHandlingExecutor(x => reset.Set());
            h.Execute(() => throw new Exception());
            Assert.IsTrue(reset.WaitOne(100));
        }

        [Test]
        public async Task AsyncExceptionHandlingExecutor()
        {
            using var reset = new AutoResetEvent(false);
            var h = new AsyncExceptionHandlingExecutor(async x => reset.Set());
            await h.Execute(async () => throw new Exception());
            Assert.IsTrue(reset.WaitOne(100));
        }
    }
}
