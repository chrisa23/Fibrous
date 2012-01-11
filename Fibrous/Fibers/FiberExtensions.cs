using System;
using Fibrous.Scheduling;

namespace Fibrous.Fibers
{
    public static class FiberExtensions
    {
        private static readonly TimerScheduler Scheduler = new TimerScheduler();

        public static IDisposable Schedule(this IFiber fiber, Action action, long firstInMs)
        {
            return Scheduler.Schedule(fiber, action, firstInMs);
        }

        public static IDisposable ScheduleOnInterval(this IFiber fiber, Action action, long firstInMs, long interval)
        {
            return Scheduler.ScheduleOnInterval(fiber, action, firstInMs, interval);
        }

        //equiv to SubscribeOnProducerThread
        public static IDisposable Subscribe<T>(this ISubscriberPort<T> port,
                                             IFiber fiber,
                                             Action<T> receive,
                                             Filter<T> filter)
        {
            Action<T> filteredReceiver = x => { if (filter(x)) fiber.Enqueue(() => receive(x)); };
            return port.Subscribe(new StubFiber(), filteredReceiver);
        }
    }
}