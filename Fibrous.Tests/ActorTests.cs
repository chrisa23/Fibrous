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

             using(var actor = Actor<string>.StartNew(x => result = x))
             {
                 actor.Send("Test");
                 Thread.Sleep(10);
             }

             Assert.AreEqual("Test",result);
         }

        [Test]
        public void ActorTest()
        {
            using (var actor = new TheActor())
            {
                actor.Start();

                actor.Send("Test");
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