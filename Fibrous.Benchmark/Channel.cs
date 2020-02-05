using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using BenchmarkDotNet.Attributes;

namespace Fibrous.Benchmark
{
    [MemoryDiagnoser]
    class Channel
    {
        private readonly IChannel<int> _channel = new Channel<int>();
        private readonly AutoResetEvent _wait = new AutoResetEvent(false);
        private int i = 0;
        private void Handler(int obj)
        {
            i++;
            if (i == 1000000)
                _wait.Set();
        }

        public void Run(IFiber fiber)
        {
            using var sub = _channel.Subscribe(fiber, Handler);
            i = 0;
            for (var j = 0; j < 1000000; j++) _channel.Publish(0);

            WaitHandle.WaitAny(new WaitHandle[] { _wait });
        }
    }
}
