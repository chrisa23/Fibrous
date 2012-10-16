using System;
using System.Diagnostics;
using NUnit.Framework;

namespace Fibrous.Tests
{
    public class ActionFactory<T>
    {
        private readonly Action<T> on;

        public ActionFactory(Action<T> onMsg)
        {
            @on = onMsg;
        }

        public Action Create(T msg)
        {
            return () => on(msg);
        }

        public Action CreateObject(object obj)
        {
            return () => on((T) obj);
        }

        public static Action Create(T msg, Action<T> target)
        {
            return () => target(msg);
        }
    }

    [TestFixture]
    public class PerfBug
    {
        public static Action CreateString(string msg, Action<string> target)
        {
            return () => target(msg);
        }

        public static Action CreateGeneric<T>(T msg, Action<T> target)
        {
            return () => target(msg);
        }

        [Test]
        public void PerfTestWithInt()
        {
            Action<int> onMsg = x => { };
            var fact = new ActionFactory<int>(onMsg);
            Stopwatch watch = Stopwatch.StartNew();
            for (int i = 0; i < 5000000; i++)
                fact.Create(1);
            watch.Stop();
            Console.WriteLine("Elapsed: " + watch.ElapsedMilliseconds);
        }

        [Test]
        public void PerfTestWithObjectString()
        {
            Action<string> onMsg = x =>
                                       {
                                           if (x == "end")
                                               Console.WriteLine(x);
                                       };
            var fact = new ActionFactory<string>(onMsg);
            Stopwatch watch = Stopwatch.StartNew();
            for (int i = 0; i < 5000000; i++)
            {
                Action act = fact.CreateObject("s");
                act();
            }
            fact.Create("end")();
            watch.Stop();
            Console.WriteLine("Elapsed: " + watch.ElapsedMilliseconds);
        }

        [Test]
        public void PerfTestWithString()
        {
            Action<string> onMsg = x =>
                                       {
                                           if (x == "end")
                                               Console.WriteLine(x);
                                       };
            var fact = new ActionFactory<string>(onMsg);
            Stopwatch watch = Stopwatch.StartNew();
            for (int i = 0; i < 5000000; i++)
            {
                Action act = fact.Create("s");
                act();
            }
            fact.Create("end")();
            watch.Stop();
            Console.WriteLine("Elapsed: " + watch.ElapsedMilliseconds);
        }

        [Test]
        public void PerfTestWithStringGenericStaticInline()
        {
            Action<string> onMsg = x => { };
            Stopwatch watch = Stopwatch.StartNew();
            for (int i = 0; i < 5000000; i++)
            {
                Action act = CreateGeneric("", onMsg);
                act();
            }
            watch.Stop();
            Console.WriteLine("Elapsed: " + watch.ElapsedMilliseconds);
        }

        [Test]
        public void PerfTestWithStringInline()
        {
            Action<string> onMsg = x =>
                                       {
                                           if (x == "end")
                                               Console.WriteLine(x);
                                       };
            Stopwatch watch = Stopwatch.StartNew();
            for (int i = 0; i < 5000000; i++)
            {
                Action act = () => onMsg(i.ToString());
                act();
            }
            Action end = () => onMsg("end");
            end();
            watch.Stop();
            Console.WriteLine("Elapsed: " + watch.ElapsedMilliseconds);
        }

        [Test]
        public void PerfTestWithStringStaticInline()
        {
            Action<string> onMsg = x => { };
            Stopwatch watch = Stopwatch.StartNew();
            for (int i = 0; i < 5000000; i++)
            {
                Action act = CreateString("", onMsg);
                act();
            }
            watch.Stop();
            Console.WriteLine("Elapsed: " + watch.ElapsedMilliseconds);
        }
    }
}