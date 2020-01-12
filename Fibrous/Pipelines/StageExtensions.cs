using System;
using System.Collections.Generic;
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
                case IHaveFiber fiber:
                    stage1.Subscribe(fiber.Fiber, stage2.Publish);
                    return new CompositeStage<T0, T1>(stage1, stage2, fiber.Fiber, stages);
                case IHaveAsyncFiber fiber:
                    stage1.Subscribe(fiber.Fiber, x =>
                    {
                        stage2.Publish(x);
                        return Task.CompletedTask;
                    });
                    return new CompositeAsyncStage<T0, T1>(stage1, stage2, fiber.Fiber, stages);
                default:
                    throw new Exception("Stage2 must implement IHaveFiber or IHaveAsyncFiber");
            }
        }

        public static IStage<T0, T> To<T0, T>(this IStage<T0, T> stage1, Action<T> f, Action<Exception> errorCallback = null)
        {
            var stage2 = new Tee<T>(f, errorCallback);
            return stage1.To(stage2);
        }

        public static IStage<T0, T> To<T0, T>(this IStage<T0, T> stage1, Func<T, Task> f, Action<Exception> errorCallback = null)
        {
            var stage2 = new AsyncTee<T>(f, errorCallback);
            return stage1.To(stage2);
        }


        public static IStage<T0, T1> To<T0, T, T1>(this IStage<T0, T> stage1, Func<T, T1> f, Action<Exception> errorCallback = null)
        {
            var stage2 = new Stage<T, T1>(f, errorCallback);
            return stage1.To(stage2);
        }
        
        public static IStage<T0, T1> To<T0, T, T1>(this IStage<T0, T> stage1, Func<T, T1> f, int count, Action<Exception> errorCallback = null)
        {
            var stages = new Disposables();
            stages.Add(stage1);
            IChannel<T1> output = new QueueChannel<T1>();
            var stub = new StubFiber();
            stages.Add(stub);
            for (int i = 0; i < count; i++)
            {
                var stage = new Stage<T, T1>(f, errorCallback);
                stages.Add(stage);
                stage1.Subscribe(stage.Fiber, stage.Publish);
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


        public static IStage<T0, T1> To<T0, T, T1>(this IStage<T0, T> stage1, Func<T, Task<T1>> f, Action<Exception> errorCallback = null)
        {
            var stage2 = new AsyncStage<T, T1>(f, errorCallback);
            return stage1.To(stage2);
        }

        public static IStage<T0, T1> To<T0, T, T1>(this IStage<T0, T> stage1, Func<T, Task<T1>> f, int count, Action<Exception> errorCallback = null)
        {
            var stages = new Disposables();
            stages.Add(stage1);
            IChannel<T1> output = new QueueChannel<T1>();
            var stub = new StubFiber();
            stages.Add(stub);
            for (int i = 0; i < count; i++)
            {
                var stage = new AsyncStage<T, T1>(f, errorCallback);
                stages.Add(stage);
                stage1.Subscribe(stage.Fiber, x =>
                {
                    stage.Publish(x);
                    return Task.CompletedTask;
                });
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