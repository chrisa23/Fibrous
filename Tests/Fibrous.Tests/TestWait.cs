using System;
using System.Threading;
using NUnit.Framework;

namespace Fibrous.Tests;

internal static class TestWait
{
    public static void For(AutoResetEvent signal, int timeoutMs = 5000) =>
        Assert.IsTrue(signal.WaitOne(timeoutMs, false));

    public static void For(ManualResetEvent signal, int timeoutMs = 5000) =>
        Assert.IsTrue(signal.WaitOne(timeoutMs, false));

    public static void For(ManualResetEventSlim signal, TimeSpan timeout) =>
        Assert.IsTrue(signal.Wait(timeout));
}
