/*
using System;
using System.Linq;
using System.Threading;
using Fibrous.Agents;
using NUnit.Framework;

namespace Fibrous.Tests;

internal class FanOutIn
{
    [Test]
    public void Run()
    {
        IFiberFactory factory = new FiberFactory();
        int OperationsPerInvoke = 1_000;
        using IChannel<string> _input = new Channel<string>();
        using IChannel<string> _queue = new QueueChannel<string>();
        using IChannel<string> _output = new Channel<string>();
        int count = 0;
        using AutoResetEvent reset = new(false);

        void Handler1(string x)
        {
            string u = x.ToUpper();
            _queue.Publish(u);
        }

        void Handler(string x)
        {
            string l = x.ToLower();
            _output.Publish(l);
        }

        void Action(string x)
        {
            int c = Interlocked.Increment(ref count);
            if (c >= OperationsPerInvoke)
            {
                reset.Set();
            }
        }


        using ChannelAgent<string> fiber = new(factory, _input, Handler1);
        IDisposable[] middle = Enumerable.Range(0, 10)
            .Select(x => new ChannelAgent<string>(factory, _queue, Handler)).ToArray();
        using ChannelAgent<string> fiberOut = new(factory, _output, Action);

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
}
*/
