using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

       
        public void Run(IFiberFactory factory)
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

    
            using var fiber = new ChannelAgent<string>(factory, _input, Handler1);
            IDisposable[] middle = Enumerable.Range(0, 10).Select(x => new ChannelAgent<string>(factory, _queue, Handler)).ToArray();
            using var fiberOut = new ChannelAgent<string>(factory, _output, Action);
            
            for (int i = 0; i < OperationsPerInvoke; i++)
            {
                _input.Publish("a");
            }

            reset.WaitOne(TimeSpan.FromSeconds(20));
            foreach (var t in middle)
            {
                t.Dispose();
            }
        }

        public void RunAsync(IFiberFactory factory)
        {
            int count = 0;
            using var reset = new AutoResetEvent(false);
            Task Handler1(string x)
            {
                var u = x.ToUpper();
                _queue.Publish(u);
                return Task.CompletedTask;
            }
            Task Handler(string x)
            {
                var l = x.ToLower();
                _output.Publish(l);
                return Task.CompletedTask;
            }
            Task Action(string x)
            {
                count++;
                if (count >= OperationsPerInvoke) reset.Set();
                return Task.CompletedTask;
            }
            void Error(Exception e ) { }

            using var fiber = new AsyncChannelAgent<string>(factory, _input, Handler1, Error);
            IDisposable[] middle = Enumerable.Range(0, 10).Select(x => new AsyncChannelAgent<string>(factory, _queue, Handler, Error)).ToArray();
            using var fiberOut = new AsyncChannelAgent<string>(factory, _output, Action, Error);

            for (int i = 0; i < OperationsPerInvoke; i++)
            {
                _input.Publish("a");
            }

            reset.WaitOne(TimeSpan.FromSeconds(20));
            foreach (var t in middle)
            {
                t.Dispose();
            }
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void Fiber()
        {
            Run(new FiberFactory());
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void LockFiber()
        {
            Run(new LockFiberFactory());
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void AsyncFiber()
        {
            RunAsync(new FiberFactory());
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void LockAsyncFiber()
        {
            RunAsync(new LockFiberFactory());
        }
    }
}
