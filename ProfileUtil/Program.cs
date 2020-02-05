using System;
using System.Threading;
using System.Threading.Tasks;
using Fibrous;

namespace ProfileUtil
{
    class Program
    {
        static void Main(string[] args)
        {
            RunSimple();
            //RunContention();
        }

        private static void RunSimple()
        {
            using var afiber = new AsyncFiber();
            using var fiber = new Fiber();
            _count = 1;
            Run(afiber);
            Run(fiber);
        }

        private static void RunContention()
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

        private const int OperationsPerInvoke = 100000;
        private static int i;
        private static readonly AutoResetEvent _wait = new AutoResetEvent(false);
        
   
        private static void Handler(object obj)
        {
            i++;
            if (i == OperationsPerInvoke)
                _wait.Set();
        }

        private static Task AsyncHandler(object obj)
        {
            i++;
            if (i == OperationsPerInvoke)
                _wait.Set();
            return Task.CompletedTask;
        }

        private static readonly IChannel<object> _channel = new Channel<object>();
        private static int _count;

        public static void Run(IFiber fiber)
        {
            using (var sub = _channel.Subscribe(fiber, Handler))
            {
                i = 0;
                for (int j = 0; j < _count; j++)
                {
                    Task.Run(Iterate);
                }
                WaitHandle.WaitAny(new WaitHandle[] { _wait });
            }
        }

        private static void Iterate()
        {
            var count = OperationsPerInvoke / _count;
            for (var j = 0; j < count; j++) _channel.Publish(null);
        }

        public static void Run(IAsyncFiber fiber)
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
