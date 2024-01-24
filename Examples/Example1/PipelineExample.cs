using System;
using System.IO;
using Example1.ComponentBased;
using Fibrous;
using Fibrous.Pipelines;

namespace Example1
{
    internal class PipelineExample
    {
        public static void Run()
        {
            RunComponentBased();
            RunSimplePipeline();
        }

        private static void RunSimplePipeline()
        {
            //Following pipeline will accept a directory
            // - enumerate it
            // - use 4 parallel workers to fan out and handle getting the file length
            // - print out the file length

            using IStage<string, (string x, long Length)[]> pipeline =
                Pipeline
                    .Start<string, string>(Directory.EnumerateFiles)
                    .SelectOrdered(x => (x, new FileInfo(x).Length), 4)
                    .Tap(info => Console.WriteLine($"**{info.x} is {info.Length} in length")) // equivalent to Select(x => {f(x); return x;})
                    .Where(x => x.Length > 1000000)
                    .Tap(info => Console.WriteLine($"{info.x} is {info.Length} in length"))
                    .Batch(TimeSpan.FromSeconds(1));
            // the resulting pipeline is still an IStage, and can be composed into another pipeline

            pipeline.Publish("C:\\");
            Console.ReadKey();
        }

        private static void RunComponentBased()
        {
            //This example uses mutable messages, which require care.  Normally you want to
            //default to using immutable messages for each stage.
            //Its possible to fan out certain stages to deal with the issue with pipelines only being
            //as fast as the slowest stage.  You can use a Queue channel say between stage1 and stage2 and then
            //instantiate more than one stage2 processor.
            var channels = new Channels();
            var processor1 = new Stage1(new SomeService());
            var processor2 = new Stage2(new SomeDataAccess());
            using var stage1 = new Component<Payload, Payload>(processor1, channels.Input, channels.Stage1To2, channels.Errors);
            using var stage2 = new Component<Payload, Payload>(processor2, channels.Stage1To2, channels.Output, channels.Errors);
            using var stub = new StubFiber();
            using var timer = new Fiber();

            channels.Output.Subscribe(stub, payload => Console.WriteLine("Got output"));
            channels.Stage1To2.Subscribe(stub, payload => Console.WriteLine("Monitoring Stage1to2 channel saw a message"));

            var twoSecs = TimeSpan.FromSeconds(2);

            timer.Schedule(() => channels.Input.Publish(new Payload()), twoSecs, twoSecs);
            Console.WriteLine("Hit any key to stop Component pipeline");
            Console.ReadKey();
        }
    }
}
