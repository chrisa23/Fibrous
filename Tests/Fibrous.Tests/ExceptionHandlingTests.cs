﻿using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Fibrous.Tests;

[TestFixture]
public class ExceptionHandlingTests
{
    [Test]
    public void ExceptionHandlingExecutor()
    {
        using AutoResetEvent reset = new(false);
        ExceptionHandlingExecutor h = new(x => reset.Set());
        h.Execute(() => throw new Exception());
        Assert.IsTrue(reset.WaitOne(100));
    }

    [Test]
    public async Task AsyncExceptionHandlingExecutor()
    {
        using AutoResetEvent reset = new(false);
#pragma warning disable 1998
        AsyncExceptionHandlingExecutor h = new(async x => reset.Set());
        await h.ExecuteAsync(async () => throw new Exception());
#pragma warning restore 1998
        Assert.IsTrue(reset.WaitOne(100));
    }
}
