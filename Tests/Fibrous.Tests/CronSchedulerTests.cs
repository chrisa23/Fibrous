using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Quartz;

namespace Fibrous.Tests;

[TestFixture]
public class CronSchedulerTests
{
    private Func<DateTimeOffset> _originalNow;

    [SetUp]
    public void SetUp() => _originalNow = SystemTime.Now;

    [TearDown]
    public void TearDown() => SystemTime.Now = _originalNow;

    [Test]
    public void CronSchedule_UsesCurrentTimeToCalculateFirstDueTime()
    {
        RecordingScheduler scheduler = new();
        SystemTime.Now = () => new DateTimeOffset(2026, 4, 13, 12, 0, 5, TimeSpan.Zero);

        using IDisposable sub = scheduler.CronSchedule(() => Task.CompletedTask, "0/10 * * * * ? *");

        Assert.That(scheduler.DueTimes, Has.Count.EqualTo(1));
        Assert.That(scheduler.DueTimes[0], Is.EqualTo(TimeSpan.FromSeconds(5)));
        Assert.That(scheduler.LastHandle.Disposed, Is.False);
    }

    [Test]
    public async Task CronSchedule_ReschedulesAfterCallbackCompletes()
    {
        RecordingScheduler scheduler = new();
        int executionCount = 0;

        SystemTime.Now = () => new DateTimeOffset(2026, 4, 13, 12, 0, 5, TimeSpan.Zero);
        using IDisposable sub = scheduler.CronSchedule(() =>
        {
            executionCount++;
            return Task.CompletedTask;
        }, "0/10 * * * * ? *");

        SystemTime.Now = () => new DateTimeOffset(2026, 4, 13, 12, 0, 10, TimeSpan.Zero);
        await scheduler.InvokeLastScheduledAsync();

        Assert.That(executionCount, Is.EqualTo(1));
        Assert.That(scheduler.DueTimes, Has.Count.EqualTo(2));
        Assert.That(scheduler.DueTimes[1], Is.EqualTo(TimeSpan.FromSeconds(10)));
        Assert.That(scheduler.Handles[0].Disposed, Is.True);
        Assert.That(scheduler.Handles[1].Disposed, Is.False);
    }

    [Test]
    public async Task CronSchedule_DisposePreventsLateQueuedCallbackFromRunning()
    {
        RecordingScheduler scheduler = new();
        int executionCount = 0;

        SystemTime.Now = () => new DateTimeOffset(2026, 4, 13, 12, 0, 5, TimeSpan.Zero);
        IDisposable sub = scheduler.CronSchedule(() =>
        {
            executionCount++;
            return Task.CompletedTask;
        }, "0/10 * * * * ? *");

        sub.Dispose();
        await scheduler.InvokeLastScheduledAsync();

        Assert.That(executionCount, Is.EqualTo(0));
        Assert.That(scheduler.DueTimes, Has.Count.EqualTo(1));
        Assert.That(scheduler.LastHandle.Disposed, Is.True);
    }

    private sealed class RecordingScheduler : IScheduler
    {
        private Func<Task> _lastAction;

        public List<TimeSpan> DueTimes { get; } = [];

        public List<RecordingDisposable> Handles { get; } = [];

        public RecordingDisposable LastHandle => Handles[^1];

        public IDisposable Schedule(Func<Task> action, TimeSpan dueTime)
        {
            _lastAction = action;
            DueTimes.Add(dueTime);

            RecordingDisposable handle = new();
            Handles.Add(handle);
            return handle;
        }

        public IDisposable Schedule(Func<Task> action, TimeSpan startTime, TimeSpan interval) =>
            throw new NotSupportedException();

        public IDisposable Schedule(Action action, TimeSpan dueTime) =>
            Schedule(action.ToAsync(), dueTime);

        public IDisposable Schedule(Action action, TimeSpan startTime, TimeSpan interval) =>
            throw new NotSupportedException();

        public Task InvokeLastScheduledAsync() => _lastAction();
    }

    private sealed class RecordingDisposable : IDisposable
    {
        public bool Disposed { get; private set; }

        public void Dispose() => Disposed = true;
    }
}
