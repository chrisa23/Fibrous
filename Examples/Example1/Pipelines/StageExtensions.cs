using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fibrous.Pipelines;

public static class Pipeline
{
    public static IStage<TIn, TOut> Start<TIn, TOut>(Func<TIn, TOut> fn, Action<Exception> errorCallback = null)
    {
        Task<TOut> Func(TIn input) { return Task.FromResult(fn(input));}
        return new AsyncStage<TIn, TOut>(Func, errorCallback);
    }
    public static IStage<TIn, TOut> Start<TIn, TOut>(Func<TIn, IEnumerable<TOut>> fn, Action<Exception> errorCallback = null)
    {
        Task<IEnumerable<TOut>> Func(TIn input) { return Task.FromResult(fn(input));}
        return new AsyncStage<TIn, TOut>(Func, errorCallback);
    }
    public static IStage<TIn, TOut> Start<TIn, TOut>(Func<TIn, Task<TOut>> fn,
        Action<Exception> errorCallback = null) => new AsyncStage<TIn, TOut>(fn, errorCallback);

    public static IStage<TIn, TOut> Start<TIn, TOut>(Func<TIn, Task<IEnumerable<TOut>>> fn,
        Action<Exception> errorCallback = null) => new AsyncStage<TIn, TOut>(fn, errorCallback);
}

public static class StageExtensions
{
    public static IStage<T0, T1> To<T0, T, T1>(this IStage<T0, T> stage1, IStage<T, T1> stage2)
    {
        Disposables stages = new();
        stages.Add(stage1);
        stages.Add(stage2);
        stages.Add(stage1.Connect(stage2));
        return new CompositeStage<T0, T1>(stage1, stage2, stages);
    }

    public static IStage<T0, T> Tap<T0, T>(this IStage<T0, T> stage1, Func<T, Task> f,
        Action<Exception> errorCallback = null) => stage1.To(new AsyncTee<T>(f, errorCallback));
    public static IStage<T0, T> Tap<T0, T>(this IStage<T0, T> stage1, Action<T> f,
        Action<Exception> errorCallback = null)
    {
        Task Func(T t)
        {
            f(t);
            return Task.CompletedTask;
        }

        return stage1.To(new AsyncTee<T>(Func, errorCallback));
    }

    public static IStage<T0, T> Where<T0, T>(this IStage<T0, T> stage1, Predicate<T> f,
        Action<Exception> errorCallback = null) => stage1.To(new Filter<T>(f, errorCallback));

    //batch
    public static IStage<T0, T[]> Batch<T0, T>(this IStage<T0, T> stage1, TimeSpan time,
        Action<Exception> errorCallback = null) => stage1.To(new Batch<T>(time, errorCallback));
    //last
    //distinct

    public static IStage<T0, T1> SelectOrdered<T0, T, T1>(this IStage<T0, T> stage1, Func<T, T1> f, int count,
        Action<Exception> errorCallback = null)
    {
        if (count < 2)
        {
            throw new ArgumentException("Count must be 2 or more", nameof(count));
        }

        OrderedRoundRobinFanOut<T> stages = new();
        stages.Add(stage1);
        stages.SetUpSubscribe(stage1);
        IChannel<T1> output = new Channel<T1>();
        OrderedJoin<T1> stub = new(output);
        stages.Add(stub);
        for (int i = 0; i < count; i++)
        {
            IStage<Ordered<T>, Ordered<T1>> stage = new AsyncStage<Ordered<T>, Ordered<T1>>(x =>
            {
                long index = x.Index;
                T1 result = f(x.Item);
                return Task.FromResult(new Ordered<T1>(index, result));
            }, errorCallback);
            stages.AddStage(stage);
            stub.Subscribe(stage);
        }

        return new CompositeStage<T0, T1>(stage1, output, stages);
    }

    public static IStage<T0, T1> Select<T0, T, T1>(this IStage<T0, T> stage1, Func<T, Task<T1>> f,
        Action<Exception> errorCallback = null) => stage1.To(new AsyncStage<T, T1>(f, errorCallback));

    public static IStage<T0, T1> SelectMany<T0, T, T1>(this IStage<T0, T> stage1, Func<T, Task<IEnumerable<T1>>> f,
        Action<Exception> errorCallback = null) => stage1.To(new AsyncStage<T, T1>(f, errorCallback));

    public static IStage<T0, T1> Select<T0, T, T1>(this IStage<T0, T> stage1, Func<T, Task<T1>> f, int count,
        Action<Exception> errorCallback = null)
    {
        if (count < 2)
        {
            throw new ArgumentException("Count must be 2 or more", nameof(count));
        }

        RoundRobinFanOut<T> stages = new(stage1);
        stages.Add(stage1);
        IChannel<T1> output = new Channel<T1>();
        for (int i = 0; i < count; i++)
        {
            AsyncStage<T, T1> stage = new(f, errorCallback);
            stages.AddStage(stage);
            stages.Add(stage.Connect(output));
        }

        return new CompositeStage<T0, T1>(stage1, output, stages);
    }

    public static IStage<T0, T1[]> Buffer<T0, T1>(this IStage<T0, T1> stage1, int size)
    {
        IChannel<T1[]> output = new Channel<T1[]>();
        Buffer<T1> stages = new(size, stage1, output);
        stages.Add(stage1);
        return new CompositeStage<T0, T1[]>(stage1, output, stages);
    }
}
