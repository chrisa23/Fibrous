using System;
using System.Collections.Generic;
using Fibrous.Channels;
using Fibrous.Fibers;
using Fibrous.Scheduling;
using NUnit.Framework;

namespace Fibrous.Tests
{
    [TestFixture]
    public class StubFiberTests
    {
        [Test]
        public void StubFiberPendingTasksShouldAllowEnqueueOfCommandsWhenExecutingAllPending()
        {
            var sut = new TestFiber {ExecutePendingImmediately = false};

            var fired1 = new object();
            var fired2 = new object();
            var fired3 = new object();

            var actionMarkers = new List<object>();

            Action command1 = delegate
                {
                    actionMarkers.Add(fired1);
                    sut.Enqueue(() => actionMarkers.Add(fired3));
                };

            Action command2 = () => actionMarkers.Add(fired2);

            sut.Enqueue(command1);
            sut.Enqueue(command2);

            sut.ExecuteAllPendingUntilEmpty();
            Assert.AreEqual(new[] {fired1, fired2, fired3}, actionMarkers.ToArray());
        }

        [Test]
        public void
            ScheduledTasksShouldBeExecutedOnceScheduleIntervalShouldBeExecutedEveryTimeExecuteScheduleAllIsCalled()
        {
            var sut = new TestFiber();
            var schedule = new TestScheduler();
            var scheduleFired = 0;
            var scheduleOnIntervalFired = 0;

            schedule.Schedule(sut, () => scheduleFired++, 100);
            var intervalSub = schedule.ScheduleOnInterval(sut, () => scheduleOnIntervalFired++, 100, 100);

            schedule.ExecuteAllScheduled();
            Assert.AreEqual(1, scheduleFired);
            Assert.AreEqual(1, scheduleOnIntervalFired);

            schedule.ExecuteAllScheduled();
            Assert.AreEqual(1, scheduleFired);
            Assert.AreEqual(2, scheduleOnIntervalFired);

            intervalSub.Dispose();

            schedule.ExecuteAllScheduled();
            Assert.AreEqual(1, scheduleFired);
            Assert.AreEqual(2, scheduleOnIntervalFired);
        }

        [Test]
        public void ShouldCompletelyClearPendingActionsBeforeExecutingNewActions()
        {
            var msgs = new List<int>();

            var sut = new TestFiber {ExecutePendingImmediately = true};
            var channel = new Channel<int>();
            const int count = 4;

            channel.Subscribe(sut, delegate(int x)
                {
                    if (x == count)
                    {
                        return;
                    }

                    channel.Publish(x + 1);
                    msgs.Add(x);
                });

            channel.Publish(0);

            Assert.AreEqual(count, msgs.Count);
            for (int i = 0; i < msgs.Count; i++)
            {
                Assert.AreEqual(i, msgs[i]);
            }
        }

        //[Test]
        //public void DisposeShouldClearAllLists()
        //{
        //    var sut = new TestFiber();
        //    var channel = new Channel<int>();

        //    channel.Subscribe(sut, x => { });
        //    // sut.Schedule(() => { }, 1000);
        //    channel.Publish(2);

        //    Assert.AreEqual(1, sut.Subscriptions.Count);
        //    //Assert.AreEqual(1,sut.Scheduler.Scheduled);
        //    Assert.AreEqual(1, sut.Pending.Count);

        //    sut.Dispose();

        //    Assert.AreEqual(0, sut.Subscriptions.Count);
        //    //Assert.AreEqual(0, sut.Scheduler.);
        //    Assert.AreEqual(0, sut.Pending.Count);
        //}
    }
}