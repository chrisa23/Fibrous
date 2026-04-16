using System;
using System.Threading;
using System.Threading.Tasks;
using Fibrous.Extras.Observability;
using NUnit.Framework;

namespace Fibrous.Tests;

[TestFixture]
public class ObservabilityTests
{
    [Test]
    public async Task ObservingExecutor_ReportsSuccessfulExecution()
    {
        ExecutionObservation observation = default;
        ObservingExecutor executor = new(x => observation = x);

        await executor.ExecuteAsync(() => Task.CompletedTask);

        Assert.IsTrue(observation.Succeeded);
        Assert.IsNull(observation.Exception);
        Assert.That(observation.Elapsed, Is.GreaterThanOrEqualTo(TimeSpan.Zero));
    }

    [Test]
    public void ObservingExecutor_ReportsFailedExecution_AndRethrows()
    {
        Exception expected = new InvalidOperationException("boom");
        ExecutionObservation observation = default;
        ObservingExecutor executor = new(x => observation = x);

        InvalidOperationException actual = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await executor.ExecuteAsync(() => Task.FromException(expected)));

        Assert.AreSame(expected, actual);
        Assert.IsFalse(observation.Succeeded);
        Assert.AreSame(expected, observation.Exception);
    }

    [Test]
    public void ObservingFiberFactory_ReportsSuccessfulWork()
    {
        ManualResetEventSlim completed = new(false);
        ExecutionObservation observation = default;
        ObservingFiberFactory factory = new(x => observation = x);
        IFiber fiber = factory.CreateFiber(error => Assert.Fail(error.ToString()));

        fiber.Enqueue(() =>
        {
            completed.Set();
            return Task.CompletedTask;
        });

        TestWait.For(completed, TimeSpan.FromSeconds(1));
        Assert.IsTrue(observation.Succeeded);
        Assert.IsNull(observation.Exception);
    }

    [Test]
    public void ObservingFiberFactory_ReportsFailedWork_AndInvokesErrorHandler()
    {
        ManualResetEventSlim observed = new(false);
        ManualResetEventSlim handled = new(false);
        ExecutionObservation observation = default;
        InvalidOperationException expected = new("boom");
        ObservingFiberFactory factory = new(x =>
        {
            observation = x;
            observed.Set();
        });

        IFiber fiber = factory.CreateFiber(error =>
        {
            Assert.AreSame(expected, error);
            handled.Set();
        });

        fiber.Enqueue(() => Task.FromException(expected));

        TestWait.For(observed, TimeSpan.FromSeconds(1));
        TestWait.For(handled, TimeSpan.FromSeconds(1));
        Assert.IsFalse(observation.Succeeded);
        Assert.AreSame(expected, observation.Exception);
    }
}
