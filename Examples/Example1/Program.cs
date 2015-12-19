using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example1
{
    using System.Security.Cryptography.X509Certificates;
    using Fibrous;


    class Program
    {
        static void Main(string[] args)
        {
            IChannel<string> toProcess = new Channel<string>();
            IChannel<string> completed = new Channel<string>();

            //first fiber will place something on channel for 2nd fiber to process
            using(IFiber fiber1 = PoolFiber.StartNew())
            using(IDisposable processor = new ChannelAgent<string>(toProcess, s => completed.Publish("Received " + s)))
            using (IDisposable logger = new ChannelAgent<string>(completed, Console.WriteLine))
            {
                int count = 0;
                //Start sending a message after a second, every 2 seconds...
                fiber1.Schedule(() => toProcess.Publish("Test" + count++), TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2));

                Console.ReadKey();
            }
        }
    }
}
