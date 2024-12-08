// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using NUnit.Framework;

namespace Fibrous.Tests;

[TestFixture]
public class SingleShotGuardTests
{
    private SingleShotGuard _guard;
    [Test]
    public void NotInitialized()
    {
        SingleShotGuard guard = new();

        Assert.IsTrue(guard.Check);

        Assert.IsFalse(guard.Check);
    }

    [Test]
    public void METHOD()
    {
        Assert.IsTrue(_guard.Check);

        Assert.IsFalse(_guard.Check);
    }
}
