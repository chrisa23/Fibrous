﻿using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Fibrous.Tests;

[TestFixture]
public class StateChannelTests
{
    [Test]
    public void StateChannel()
    {
        using AsyncFiber fiber = new();
        string result = null;
        AutoResetEvent reset = new(false);

        Task Handle(string s)
        {
            result = s;
            reset.Set();
            return Task.CompletedTask;
        }

        StateChannel<string> channel = new("none");
        TimeSpan fromSeconds = TimeSpan.FromSeconds(1);
        using (IDisposable sub = channel.Subscribe(fiber, Handle))
        {
            Assert.IsTrue(reset.WaitOne(fromSeconds));
            Assert.AreEqual("none", result);
            channel.Publish("one");
            Assert.IsTrue(reset.WaitOne(fromSeconds));
            Assert.AreEqual("one", result);
        }

        result = null;
        channel.Publish("two");
        Thread.Sleep(100);
        Assert.IsNull(result);
        using (IDisposable sub = channel.Subscribe(fiber, Handle))
        {
            Assert.IsTrue(reset.WaitOne(fromSeconds));
            Assert.AreEqual("two", result);
            channel.Publish("three");
            Assert.IsTrue(reset.WaitOne(fromSeconds));
            Assert.AreEqual("three", result);
        }
    }

    [Test]
    public void StateChannelNoInit()
    {
        using AsyncFiber fiber = new();
        string result = null;
        AutoResetEvent reset = new(false);

        Task Handle(string s)
        {
            result = s;
            reset.Set();
            return Task.CompletedTask;
        }

        StateChannel<string> channel = new();
        TimeSpan fromSeconds = TimeSpan.FromSeconds(0.1);
        using (IDisposable sub = channel.Subscribe(fiber, Handle))
        {
            Assert.IsFalse(reset.WaitOne(fromSeconds));
            Assert.IsNull(result);
            channel.Publish("one");
            Assert.IsTrue(reset.WaitOne(fromSeconds));
            Assert.AreEqual("one", result);
        }

        result = null;
        channel.Publish("two");
        Thread.Sleep(100);
        Assert.IsNull(result);
        using (IDisposable sub = channel.Subscribe(fiber, Handle))
        {
            Assert.IsTrue(reset.WaitOne(fromSeconds));
            Assert.AreEqual("two", result);
            channel.Publish("three");
            Assert.IsTrue(reset.WaitOne(fromSeconds));
            Assert.AreEqual("three", result);
        }
    }

    [Test]
    public void AsyncStateChannel()
    {
        using AsyncFiber fiber = new();
        string result = null;
        AutoResetEvent reset = new(false);

        Task Handle(string s)
        {
            result = s;
            reset.Set();
            return Task.CompletedTask;
        }

        StateChannel<string> channel = new("none");
        TimeSpan fromSeconds = TimeSpan.FromSeconds(1);
        using (IDisposable sub = channel.Subscribe(fiber, Handle))
        {
            Assert.IsTrue(reset.WaitOne(fromSeconds));
            Assert.AreEqual("none", result);
            channel.Publish("one");
            Assert.IsTrue(reset.WaitOne(fromSeconds));
            Assert.AreEqual("one", result);
        }

        result = null;
        channel.Publish("two");
        Thread.Sleep(100);
        Assert.IsNull(result);
        using (IDisposable sub = channel.Subscribe(fiber, Handle))
        {
            Assert.IsTrue(reset.WaitOne(fromSeconds));
            Assert.AreEqual("two", result);
            channel.Publish("three");
            Assert.IsTrue(reset.WaitOne(fromSeconds));
            Assert.AreEqual("three", result);
        }
    }

    [Test]
    public void AsyncStateChannelNoInit()
    {
        using AsyncFiber fiber = new();
        string result = null;
        AutoResetEvent reset = new(false);

        Task Handle(string s)
        {
            result = s;
            reset.Set();
            return Task.CompletedTask;
        }

        StateChannel<string> channel = new();
        TimeSpan fromSeconds = TimeSpan.FromSeconds(0.1);
        using (IDisposable sub = channel.Subscribe(fiber, Handle))
        {
            Assert.IsFalse(reset.WaitOne(fromSeconds));
            Assert.IsNull(result);
            channel.Publish("one");
            Assert.IsTrue(reset.WaitOne(fromSeconds));
            Assert.AreEqual("one", result);
        }

        result = null;
        channel.Publish("two");
        Thread.Sleep(100);
        Assert.IsNull(result);
        using (IDisposable sub = channel.Subscribe(fiber, Handle))
        {
            Assert.IsTrue(reset.WaitOne(fromSeconds));
            Assert.AreEqual("two", result);
            channel.Publish("three");
            Assert.IsTrue(reset.WaitOne(fromSeconds));
            Assert.AreEqual("three", result);
        }
    }
}
