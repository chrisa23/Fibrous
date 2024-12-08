using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Fibrous.Agents;

namespace Fibrous.Benchmark
{
    [MemoryDiagnoser]
    public class FanOutIn
    {
        private const int OperationsPerInvoke = 1_000;


        public void Run(IFiberFactory factory)
        {
            using IChannel<string> _input = new Channel<string>();
            using IChannel<string> _queue = new QueueChannel<string>();
            using IChannel<string> _output = new Channel<string>();
            int count = 0;
            using AutoResetEvent reset = new(false);

            async Task Handler1(string x)
            {
                string u = x.ToUpper();
                _queue.Publish(u);
            }

            async Task Handler(string x)
            {
                string l = x.ToLower();
                _output.Publish(l);
            }

            async Task Action(string x)
            {
                count++;
                if (count == OperationsPerInvoke)
                {
                    reset.Set();
                }
            }


            using ChannelAgent<string> fiber = new(factory, _input, Handler1, e => { });
            IDisposable[] middle = Enumerable.Range(0, 4)
                .Select(x => new ChannelAgent<string>(factory, _queue, Handler, e => { })).ToArray();
            using ChannelAgent<string> fiberOut = new(factory, _output, Action, e => { });

            for (int i = 0; i < OperationsPerInvoke; i++)
            {
                _input.Publish("a");
            }

            reset.WaitOne(TimeSpan.FromSeconds(20));
            foreach (IDisposable t in middle)
            {
                t.Dispose();
            }
        }

        public void RunAsync(IFiberFactory factory)
        {
            using IChannel<string> _input = new Channel<string>();
            using IChannel<string> _queue = new QueueChannel<string>();
            using IChannel<string> _output = new Channel<string>();
            int count = 0;
            using AutoResetEvent reset = new(false);

            Task Handler1(string x)
            {
                string u = x.ToUpper();
                _queue.Publish(u);
                return Task.CompletedTask;
            }

            Task Handler(string x)
            {
                string l = x.ToLower();
                _output.Publish(l);
                return Task.CompletedTask;
            }

            Task Action(string x)
            {
                count++;
                if (count == OperationsPerInvoke)
                {
                    reset.Set();
                }

                return Task.CompletedTask;
            }

            void Error(Exception e)
            {
            }

            using ChannelAgent<string> fiber = new(factory, _input, Handler1, Error);
            IDisposable[] middle = Enumerable.Range(0, 4)
                .Select(x => new ChannelAgent<string>(factory, _queue, Handler, Error)).ToArray();
            using ChannelAgent<string> fiberOut = new(factory, _output, Action, Error);

            for (int i = 0; i < OperationsPerInvoke; i++)
            {
                _input.Publish("a");
            }

            reset.WaitOne(TimeSpan.FromSeconds(20));
            foreach (IDisposable t in middle)
            {
                t.Dispose();
            }
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void Fiber() => Run(new FiberFactory());

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void LockFiber() => Run(new LockFiberFactory());

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void AsyncFiber() => RunAsync(new FiberFactory());

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void LockAsyncFiber() => RunAsync(new LockFiberFactory());
    }
}
