using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Fibrous.Tests;

[TestFixture]
public class ExceptionHandlingTests
{

    [Test]
    public async Task AsyncExceptionHandlingExecutor()
    {
        using AutoResetEvent reset = new(false);
#pragma warning disable 1998
        ExceptionHandlingExecutor h = new(async x => reset.Set());
        await h.ExecuteAsync(async () => throw new Exception());
#pragma warning restore 1998
        Assert.IsTrue(reset.WaitOne(100));
    }
}
