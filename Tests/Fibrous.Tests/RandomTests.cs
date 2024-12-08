using System;
using System.Threading;
using System.Threading.Tasks;
using Fibrous.Agents;
using NUnit.Framework;

namespace Fibrous.Tests;

[TestFixture]
internal class RandomTests
{
    private const int OperationsPerInvoke = 100000;
    private readonly IChannel<string> _input = new Channel<string>();
    private readonly IChannel<string> _queue = new QueueChannel<string>();
    private readonly IChannel<string> _output = new Channel<string>();

    [Test]
    public void FanOutIn()
    {
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

        async Task Action(string x)
        {
            count++;
            if (count >= OperationsPerInvoke)
            {
                reset.Set();
            }
        }

        using Disposables d = new();
        using ChannelAgent<string> fiber = new(_input, Handler1, e => { });//Todo: make sure no exceptions
        for (int i = 0; i < 10; i++)
        {
            d.Add(new ChannelAgent<string>(_queue, Handler, e => { }));
        }

        using ChannelAgent<string> fiberOut = new(_output,  (Func<string, Task>)Action, e => { });

        for (int i = 0; i < OperationsPerInvoke; i++)
        {
            _input.Publish("a");
        }

        Assert.IsTrue(reset.WaitOne(TimeSpan.FromSeconds(20)));
    }

    [Test]
    public void GuardTest()
    {
        SingleShotGuard singleShot = new();
        Assert.IsTrue(singleShot.Check);
        Assert.IsFalse(singleShot.Check);
    }
}
