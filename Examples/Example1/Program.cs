using System;
using Fibrous;
using Fibrous.Agents;

namespace Example1
{
    internal class Program
    {
        private static void Main()
        {
            using var fiber = new Fiber();
            var channel = fiber.NewChannel<string>(Console.WriteLine);

            channel.Publish("Test message");

            var oneSec = TimeSpan.FromSeconds(1);
            fiber.Schedule(() => Console.WriteLine("this was scheduled"), oneSec, oneSec);

            Console.ReadKey();
        }

        private static void AgentExample()
        {
            using IAgent<string> logger = new Agent<string>(Console.WriteLine);
            using IAgent<string> processor = new Agent<string>(s => logger.Publish("Received " + s));
            using var fiber1 = new Fiber();

            var count = 0;
            //Start sending a message after a second, every 2 seconds...
            fiber1.Schedule(() => processor.Publish("Test" + count++), TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2));
        }
    }
}