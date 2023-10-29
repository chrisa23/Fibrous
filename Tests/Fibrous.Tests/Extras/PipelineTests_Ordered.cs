using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fibrous.Pipelines;
using NUnit.Framework;

namespace Fibrous.Tests;

[TestFixture]
public class PipelineTests_Ordered
{
    [Test]
    public async Task Basic()
    {
        using AutoResetEvent reset = new(false);
        long index = 0;
        int count = 1000;
        IStage<int, int> pipe = new Stage<int, int>(x => Enumerable.Range(0, count).ToArray())
            .SelectOrdered(x => x, 4);
        using Fiber fiber = new();
        pipe.Subscribe(fiber, x =>
        {
            Assert.AreEqual(index, x);
            index++;
            if (index == count)
            {
                reset.Set();
            }
        });
        pipe.Publish(0);
        await Task.Delay(TimeSpan.FromSeconds(1));
        Console.WriteLine(index);
        Assert.IsTrue(reset.WaitOne(10000, false));
    }

    [Test]
    public async Task JaggedTimes()
    {
        using AutoResetEvent reset = new(false);

        long index = 0;
        int count = 1000;
        IStage<int, int> pipe = new Stage<int, int>(x => Enumerable.Range(0, count).ToArray())
            .SelectOrdered(x =>
            {
                Random rnd = new(x);
                Thread.Sleep((int)(rnd.NextDouble() * 10));
                return x;
            }, 4);
        using Fiber fiber = new();
        pipe.Subscribe(fiber, x =>
        {
            Assert.AreEqual(index, x);
            index++;
            if (index == count)
            {
                reset.Set();
            }
        });
        pipe.Publish(0);
        await Task.Delay(TimeSpan.FromSeconds(1));
        Console.WriteLine(index);
        Assert.IsTrue(reset.WaitOne(10000, false));
    }

    [Test]
    public async Task Simple()
    {
        long index = 0;

        static int Id(int i)
        {
            return i;
        }

        static void Handle(Exception e)
        {
            Console.WriteLine(e);
        }

        using AutoResetEvent reset = new(false);
        using IStage<int, int> pipe = new Stage<int, int>(Id, Handle)
            .Select(Id, Handle)
            .Select(Id, Handle)
            .Select(Id, Handle);
        using Fiber fiber = new();
        pipe.Subscribe(fiber, x =>
        {
            index++;
            if (index == 100000)
            {
                reset.Set();
            }
        });
        for (int i = 0; i < 100000; i++)
        {
            pipe.Publish(i);
        }

        await Task.Delay(TimeSpan.FromSeconds(1));
        Console.WriteLine(index);
        Assert.IsTrue(reset.WaitOne(10000, false));
    }

    [Test]
    public void Complex1()
    {
        long index = 0;
        using AutoResetEvent reset = new(false);
        using IStage<int, int> pipe = new Stage<int, int>(x => x)
            .SelectOrdered(x => x, 4)
            .Where(x => x % 2 == 0)
            .Select(x => x)
            .Tap(x =>
            {
                if (x % 10000 == 0)
                {
                    Console.WriteLine(x);
                }
            });

        pipe.Subscribe(x =>
        {
            index++;
            if (index % 10000 == 0)
            {
                Console.WriteLine("I" + index);
            }

            if (index == OperationsPerInvoke / 2)
            {
                reset.Set();
            }
        });
        for (int i = 1; i <= OperationsPerInvoke; i++)
        {
            pipe.Publish(i);
        }

        Assert.IsTrue(reset.WaitOne(10000, false));
    }

    public const long OperationsPerInvoke = 100000;
}
