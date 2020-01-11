using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Fibrous.Tests
{
    [TestFixture]
    public class ContentionTests
    {
        private const int OperationsPerInvoke = 10000000;
        private int i;
        private readonly AutoResetEvent _wait = new AutoResetEvent(false);
        [Test]
        public void Test()
        {
            using var afiber = new AsyncFiber();
            using var fiber = new Fiber();
            _count = 2;
            Run(afiber);
            Run(fiber);

            _count = 4;
            Run(afiber);
            Run(fiber);

            _count = 10;
            Run(afiber);
            Run(fiber);
        }
        private void Handler(object obj)
        {
            i++;
            if (i == OperationsPerInvoke)
                _wait.Set();
        }

        private Task AsyncHandler(object obj)
        {
            i++;
            if (i == OperationsPerInvoke)
                _wait.Set();
            return Task.CompletedTask;
        }

        private readonly IChannel<object> _channel = new Channel<object>();
        private int _count;

        public void Run(IFiber fiber)
        {
            using (var sub = _channel.Subscribe(fiber, Handler))
            {
                i = 0;
                for (int j = 0; j < _count; j++)
                {
                    Task.Run(Iterate);
                }
                
                //Task.Run(Iterate);

                WaitHandle.WaitAny(new WaitHandle[] { _wait });
            }
        }

        private void Iterate()
        {
            var count = OperationsPerInvoke / _count;
            for (var j = 0; j < count; j++) _channel.Publish(null);
        }

        public void Run(IAsyncFiber fiber)
        {
            using (var sub = _channel.Subscribe(fiber, AsyncHandler))
            {
                i = 0;
                for (int j = 0; j < _count; j++)
                {
                    Task.Run(Iterate);
                }
                
                WaitHandle.WaitAny(new WaitHandle[] { _wait });
            }
        }

    }
}
