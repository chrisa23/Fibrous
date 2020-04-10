using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Fibrous.Agents;

namespace Fibrous.Benchmark
{
    [MemoryDiagnoser]
    public class Agent
    {
        private const int OperationsPerInvoke = 10000000;
        private readonly AutoResetEvent _wait = new AutoResetEvent(false);
        private int i;

        private void Handler(int obj)
        {
            i++;
            if (i == OperationsPerInvoke)
                _wait.Set();
        }

        private Task AsyncHandler(int obj)
        {
            i++;
            if (i == OperationsPerInvoke)
                _wait.Set();
            return Task.CompletedTask;
        }
        
        public void Run(IAgent<int> agent)
        {
            using (agent)
            {
                i = 0;
                for (var j = 0; j < OperationsPerInvoke; j++) agent.Publish(j);
                WaitHandle.WaitAny(new WaitHandle[] { _wait });
            }
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void Agent1()
        {
            Run(new Agent<int>(Handler));
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void AsyncAgent()
        {
            Run(new AsyncAgent<int>(AsyncHandler, ex => { }));
        }


    }
}
