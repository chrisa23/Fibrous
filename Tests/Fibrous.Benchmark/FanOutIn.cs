using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using BenchmarkDotNet.Attributes;
using Fibrous.Agents;

namespace Fibrous.Benchmark
{
    [MemoryDiagnoser]
    public class FanOutIn
    {
        private const int OperationsPerInvoke = 1000000;
        private readonly IChannel<string> _input = new Channel<string>();
        private readonly IChannel<string> _queue = new QueueChannel<string>();
        private readonly IChannel<string> _output = new Channel<string>();

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void Pool2()
        {
            int count = 0;
            using var reset = new AutoResetEvent(false);
            void Handler1(string x)
            {
                var u = x.ToUpper();
                _queue.Publish(u);
            }
            void Handler(string x)
            {
                var l = x.ToLower();
                _output.Publish(l);
            }
            void Action(string x)
            {
                count++;
                if (count >= OperationsPerInvoke) reset.Set();
            }

    
            using var fiber = new ChannelAgent<string>(_input, Handler1);
            IDisposable[] middle = Enumerable.Range(0, 10).Select(x => new ChannelAgent<string>(_queue, Handler)).ToArray();
            using var fiberOut = new ChannelAgent<string>(_output, Action);
            
            for (int i = 0; i < OperationsPerInvoke; i++)
            {
                _input.Publish("a");
            }

            reset.WaitOne(TimeSpan.FromSeconds(20));
            for (int i = 0; i < middle.Length; i++)
            {
                middle[i].Dispose();
            }
        }
    }
}
