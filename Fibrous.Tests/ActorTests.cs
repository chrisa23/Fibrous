namespace Fibrous.Tests
{
    using System.Threading;
    using Fibrous.Actors;
    using NUnit.Framework;

    [TestFixture]
    public class ActorTests
    {
        [Test]
        public void AnonymousActorTest()
        {
            string result = "";
            using (IActor<string> actor = Actor<string>.Start(x => result = x))
            {
                actor.Publish("Test");
                Thread.Sleep(15);
            }
            Assert.AreEqual("Test", result);
        }

        [Test]
        public void ActorTest()
        {
            using (var actor = new TheActor())
            {
                actor.Start();
                actor.Publish("Test");
                Thread.Sleep(10);
                Assert.AreEqual("Test", actor.Result);
            }
        }

        private sealed class TheActor : ActorBase<string>
        {
            public string Result;

            protected override void OnMessage(string msg)
            {
                Result = msg;
            }
        }
    }
}