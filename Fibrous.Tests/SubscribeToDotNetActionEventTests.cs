using System;
using NUnit.Framework;

namespace Fibrous.Tests
{
    [TestFixture]
    public class SubscribeToDotNetActionEventTests
    {
        private class EventTester
        {
            public bool IsAttached => Event != null;
            public event Action<object> Event;

            public void Invoke()
            {
                Event?.Invoke(null);
            }
        }

        [Test]
        public void CanSubscribeToEvent()
        {
            var triggered = false;
            var stub = new StubFiber();
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