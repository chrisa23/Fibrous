using System;
using System.Threading.Tasks;

namespace Fibrous.Pipelines
{
    public static class StageExtensions
    {
        public static IStage<T0, T1> To<T0, T, T1>(this IStage<T0, T> stage1, IStage<T, T1> stage2)
        {
            var stages = new Disposables();
            stages.Add(stage1);
            stages.Add(stage2);
            switch (stage2)
            {
                case IHaveFiber fiber2:
                    stage1.Subscribe(fiber2.Fiber, stage2.Publish);
                    break;
                case IHaveAsyncFiber fiber2:
                    stage1.Subscribe(fiber2.Fiber, x =>
                    {
                        stage2.Publish(x);
                        return Task.CompletedTask;
                    });
                    break;
                default:
                    throw new Exception("Stage2 must implement IHaveFiber or IHaveAsyncFiber");
            }

            switch (stage1)
            {
                case IHaveFiber fiber:
                    return new CompositeStage<T0, T1>(stage1, stage2, fiber.Fiber, stages);
                case IHaveAsyncFiber fiber:
                    return new CompositeAsyncStage<T0, T1>(stage1, stage2, fiber.Fiber, stages);
                default:
                    throw new Exception("Stage1 must implement IHaveFiber or IHaveAsyncFiber");
            }
        }

        public static IStage<T0, T> Tap<T0, T>(this IStage<T0, T> stage1, Action<T> f, Action<Exception> errorCallback = null)
        {
            var stage2 = new Tee<T>(f, errorCallback);
            return stage1.To(stage2);
        }

        public static IStage<T0, T> Tap<T0, T>(this IStage<T0, T> stage1, Func<T, Task> f, Action<Exception> errorCallback = null)
        {
            var stage2 = new AsyncTee<T>(f, errorCallback);
            return stage1.To(stage2);
        }

        public static IStage<T0, T> Where<T0, T>(this IStage<T0, T> stage1, Predicate<T> f, Action<Exception> errorCallback = null)
        {
            var stage2 = new Filter<T>(f, errorCallback);
            return stage1.To(stage2);
        }

        //batch
        public static IStage<T0, T[]> Batch<T0, T>(this IStage<T0, T> stage1, TimeSpan time, Action<Exception> errorCallback = null)
        {
            var stage2 = new Batch<T>(time, errorCallback);
            return stage1.To(stage2);
        }
        //last
        //

        public static IStage<T0, T1> Select<T0, T, T1>(this IStage<T0, T> stage1, Func<T, T1> f, Action<Exception> errorCallback = null)
        {
            var stage2 = new Stage<T, T1>(f, errorCallback);
            return stage1.To(stage2);
        }

        public static IStage<T0, T1> Select<T0, T, T1>(this IStage<T0, T> stage1, Func<T, T1> f, int count, Action<Exception> errorCallback = null)
        {
            //TODO: assert if count < 2
            var stages = new RoundRobinFanOut<T>();
            stages.Add(stage1);
            stages.SetUpSubscribe(stage1);
            IChannel<T1> output = new Channel<T1>();
            var stub = new StubFiber();
            stages.Add(stub);
            for (int i = 0; i < count; i++)
            {
                var stage = new Stage<T, T1>(f, errorCallback);
                stages.AddStage(stage);
                // this is the part of the Fan OUt stage1.Subscribe(stage.Fiber, stage.Publish);
                stage.Subscribe(stub, output.Publish);
            }

            switch (stage1)
            {
                case IHaveFiber fiber:
                    return new CompositeStage<T0, T1>(stage1, output, fiber.Fiber, stages);
                case IHaveAsyncFiber fiber:
                    return new CompositeAsyncStage<T0, T1>(stage1, output, fiber.Fiber, stages);
                default:
                    throw new Exception("Stage1 must implement IHaveFiber or IHaveAsyncFiber");
            }
        }


        public static IStage<T0, T1> Select<T0, T, T1>(this IStage<T0, T> stage1, Func<T, Task<T1>> f, Action<Exception> errorCallback = null)
        {
            var stage2 = new AsyncStage<T, T1>(f, errorCallback);
            return stage1.To(stage2);
        }

        public static IStage<T0, T1> Select<T0, T, T1>(this IStage<T0, T> stage1, Func<T, Task<T1>> f, int count, Action<Exception> errorCallback = null)
        {
            var stages = new RoundRobinFanOut<T>();
            stages.Add(stage1);
            stages.SetUpSubscribe(stage1);
            IChannel<T1> output = new Channel<T1>();
            var stub = new StubFiber();
            stages.Add(stub);
            for (int i = 0; i < count; i++)
            {
                var stage = new AsyncStage<T, T1>(f, errorCallback);
                stages.AddStage(stage);
            
                stage.Subscribe(stub, output.Publish);
            }

            switch (stage1)
            {
                case IHaveFiber fiber:
                    return new CompositeStage<T0, T1>(stage1, output, fiber.Fiber, stages);
                case IHaveAsyncFiber fiber:
                    return new CompositeAsyncStage<T0, T1>(stage1, output, fiber.Fiber, stages);
                default:
                    throw new Exception("Stage1 must implement IHaveFiber or IHaveAsyncFiber");
            }
        }
    }
}