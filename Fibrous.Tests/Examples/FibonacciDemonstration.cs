namespace Fibrous.Tests.Examples
{
    using System;
    using System.Threading;
    using Fibrous.Channels;
    using Fibrous.Fibers;
    using NUnit.Framework;

    [TestFixture]
    [Category("Demo")]
    //[Ignore("Demo")]
    public class FibonacciDemonstration
    {
        // Simple immutable class that serves as a message
        // to be passed between services.
        private class IntPair
        {
            private readonly int _first;
            private readonly int _second;

            public IntPair(int first, int second)
            {
                _first = first;
                _second = second;
            }

            public int First
            {
                get
                {
                    return _first;
                }
            }
            public int Second
            {
                get
                {
                    return _second;
                }
            }
        }

        // This class calculates the next value in a Fibonacci sequence.
        // It listens for the previous pair on one topic, and then publishes
        // a new pair with the latest value onto the reply topic.
        // When a specified limit is reached, it stops processing.
        private class FibonacciCalculator
        {
            private readonly IFiber _threadFiber;
            private readonly string _name;
            private readonly IChannel<IntPair> _inboundChannel;
            private readonly IChannel<IntPair> _outboundChannel;
            private readonly int _limit;

            public FibonacciCalculator(IFiber fiber,
                                       string name,
                                       IChannel<IntPair> inboundChannel,
                                       IChannel<IntPair> outboundChannel,
                                       int limit)
            {
                _threadFiber = fiber;
                _name = name;
                _inboundChannel = inboundChannel;
                _outboundChannel = outboundChannel;
                _inboundChannel.Subscribe(fiber, CalculateNext);
                _limit = limit;
            }

            public void Begin(IntPair pair)
            {
                Console.WriteLine(_name + " " + pair.Second);
                _outboundChannel.Send(pair);
            }

            private void CalculateNext(IntPair receivedPair)
            {
                int next = receivedPair.First + receivedPair.Second;
                var pairToPublish = new IntPair(receivedPair.Second, next);
                _outboundChannel.Send(pairToPublish);
                if (next > _limit)
                {
                    Console.WriteLine("Stopping " + _name);
                    _threadFiber.Dispose();
                    return;
                }
                Console.WriteLine(_name + " " + next);
            }
        }

        [Test]
        public void DoDemonstration()
        {
            // Two instances of the calculator are created.  One is named "Odd" 
            // (it calculates the 1st, 3rd, 5th... values in the sequence) the
            // other is named "Even".  They message each other back and forth
            // with the latest two values and successively build the sequence.
            int limit = 1000;
            // Two channels for communication.  Naming convention is inbound.
            var oddChannel = new Channel<IntPair>();
            var evenChannel = new Channel<IntPair>();
            using (IFiber oddFiber = ThreadFiber.StartNew(), evenFiber = ThreadFiber.StartNew())
            {
                var oddCalculator = new FibonacciCalculator(oddFiber, "Odd", oddChannel, evenChannel, limit);
                var evenCalculator = new FibonacciCalculator(evenFiber, "Even", evenChannel, oddChannel, limit);
                oddCalculator.Begin(new IntPair(0, 1));
                Thread.Sleep(100);
            }
        }
    }
}