namespace Fibrous.Tests
{
    using System;
    using Fibrous.Fibers;
    using NUnit.Framework;

    [TestFixture]
    public class SubscribeToDotNetActionEventTests
    {
        private class EventTester
        {
            public event Action<object> Event;

            public void Invoke()
            {
                Event?.Invoke(null);
            }

            public bool IsAttached => Event != null;
        }

        [Test]
        public void CanSubscribeToEvent()
        {
            bool triggered = false;
            var stub = StubFiber.StartNew();
            var evt = new EventTester();
            var dispose = stub.SubscribeToEvent<object>(evt, "Event", x => triggered = true);
            Assert.IsTrue(evt.IsAttached);
            evt.Invoke();
            Assert.IsTrue(triggered);
            dispose.Dispose();
            Assert.IsFalse(evt.IsAttached);
        }
    }
}