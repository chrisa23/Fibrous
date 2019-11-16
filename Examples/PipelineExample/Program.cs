namespace PipelineExample
{
    using System;
    using Fibrous;
    using Fibrous.Pipeline;

    class Program
    {
        static void Main(string[] args)
        {
            //This example uses mutable messages, which require care.  Normally you want to 
            //default to using immutable messages for each stage.
            //Its possible to fan out certain stages to deal with the issue with pipelines only being 
            //as fast as the slowest stage.  You can use a Queue channel say between stage1 and stage2 and then 
            //instantiate more than one stage2 processor.
            var channels = new Channels();
            using (var stage1 = new ConcurrentComponent<Payload, Payload>(new Stage1(new SomeService()), channels.Input, channels.Stage1To2, channels.Errors))
            using (var stage2 = new ConcurrentComponent<Payload, Payload>(new Stage2(new SomeDataAccess()), channels.Stage1To2, channels.Output, channels.Errors))
            using (var stub = StubFiber.StartNew())
            {
                channels.Output.Subscribe(stub, payload => Console.WriteLine("Got output"));
                channels.Stage1To2.Subscribe(stub, payload => Console.WriteLine("Monitoring Stage1to2 channel saw a message"));
                using (var timer = new ThreadSafeTimer())
                {
                    timer.Schedule(() => channels.Input.Publish(new Payload()), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(2));
                    Console.WriteLine("Hit any key to stop");
                    Console.ReadKey();
                }
            }
        }
    }
}