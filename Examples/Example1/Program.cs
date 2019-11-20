using System;
using Fibrous;
using Fibrous.Agents;

namespace Example1
{
    internal class Program
    {
        private static void Main()
        {

            using (IAgent<string> logger = new Agent<string>(Console.WriteLine))
            using (IAgent<string> processor = new Agent<string>(s => logger.Publish("Received " + s)))
            
            using (var fiber1 = Fiber.StartNew())
            {
                var count = 0;
                //Start sending a message after a second, every 2 seconds...
                fiber1.Schedule(() => processor.Publish("Test" + count++), TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(2));
                Console.ReadKey();
            }
        }
    }
}