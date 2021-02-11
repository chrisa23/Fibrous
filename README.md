# Fibrous

[![NuGet](https://img.shields.io/nuget/v/Fibrous.svg)](https://www.nuget.org/packages/Fibrous/)

High performance concurrency library for the .Net platform.  Fibrous is a fork of Retlang [http://code.google.com/p/retlang/]. 

Fibrous is an actor-like framework but more of a flexible set of concurrency components. The main abstractions are Fibers, Ports and Channels.  Fibers are execution contexts, Ports are messaging end points, and channels are combinations of ports that allow decoupling of fiber components.

Some of the library benefits:
 - Tiny library that makes multi-threading simple and easy to reason about
 - Thread safe publishing
 - Single or multiple subscribers
 - Request reply
 - UI fibers for worry free UI marshalling
 - Batching support
 - Scheduling support (Cron, DateTime and TimeSpan based)
 
 Fibrous is great for multi-threading when you don't need extreme low latency or distributed actors but want an easy to reason about and extremely flexible messaging based model.  Fibrous is also fast.  It's in production use in multiple trading systems, both server side and front end.
  
 If you need distributed concurrency, look into Akka.net or Proto.Actor and if you need extreme performance and super low latency, look into Disruptor.net.

Fibers
------

Fibers are synchronous execution contexts that maintain order of actions.  Like Actors, Fibers can manage state without worries of cross threading issues.  While a Fiber is synchronous, your system can consist of multiple Fibers communicating through messaging to provide parallelism to your system.

In version 3, Async Fibers were introduced.  These work off of Func&lt;Task> rather than Action delegates allowing a fiber based component to use async/await safely.

Fibers subscribe to channels to receive messages which queue actions based on the assigned handler.  Fibers have a scheduling API that allows actions to be scheduled in the future as well as repeatedly.  You can also directly queue actions onto a Fiber for it to execute.

Fibers are a repository of IDisposable objects and will dispose of all children upon the Fibers disposal.  This is used to clean up subscriptions and scheduling for any fiber.  This is also useful for dealing with children used in the Fiber's context that implement IDisposable.

There are specialised Fibers for Windows Forms and WPF, which automatically handle invoking actions on the UI/Dispatcher thread.  There is a StubFiber, which is used for testing and special cases, and immediately executes actions on the calling thread.

```csharp
//Representations of the IFiber and IAsyncFiber interface.
//There are many extensions to enable more complex behavior
public interface IFiber : IDisposable
{
    void Enqueue(Action action);
    IDisposable Schedule(Action action, TimeSpan dueTime);
    IDisposable Schedule(Action action, TimeSpan startTime, TimeSpan interval);
    void Add(IDisposable toAdd);
    void Remove(IDisposable toRemove);
}

public interface IAsyncFiber : IDisposable
{
    void Enqueue(Func<Task> action);
    IDisposable Schedule(Func<Task> action, TimeSpan dueTime);
    IDisposable Schedule(Func<Task> action, TimeSpan startTime, TimeSpan interval);
    void Add(IDisposable toAdd);
    void Remove(IDisposable toRemove);
}
```

```csharp
//Work is done on the thread pool, but in a sequential fashion 
IFiber fiber = new Fiber();  
	 
//You can enqueue methods
fiber.Enqueue(SomeParameterlessMethod);
 
//or lambdas
fiber.Enqueue(() => DoSomeWork(someParam));

//You can schedule when things happen
fiber.Schedule(ScheduledMethod, when);

//and also have them repeat
fiber.Schedule(ScheduledMethod, startWhen, repeatInterval);
```



Ports and Channels
------------------

Ports are the end points for publishing and subscribing to messages.  

The port types (ISubscriberPort<T>, IPublisherPort<T>, IRequestPort<TRequest, TReply>, ISnapshotSubscriberPort<T, TSnapshot>, IEventPort and IEventTrigger) allow a variety of behavior around message passing and events.
    
Channels are the conduit for message passing between Fibers, and allow decoupling of the parts of your system.  There are a variety of channel types built into Fibrous: one way channels that notify all subscribers, request/reply, queue channels that give messages to one of multiple subscribers, as well as a state channel that acts like a last value cache.

There is a static EventBus which allows a simpler mechanism for passing messages when only one normal channel per type is needed and an EventHub that allows auto-wiring of handlers.

There are a variety of subscription methods, including filtered, batched, keyed batched and the last message after a time interval.
 
  
Examples:

```csharp
IFiber fiber = new Fiber();  
	 
//Create a channel and subscribe to messages
IChannel<string> channel = new Channel<string>();

channel.Subscribe(fiber, s => Console.WriteLine(s.ToUpper()));

//You can also create a channel and subscribe in one line
IChannel<string> channel = fiber.NewChannel<string>(s => Console.WriteLine(s.ToUpper()));

//Publish a message to the channel
channel.Publish("the message");

//EventBus... Global static channel per type (string is only used as an example)
EventBus<string>.Subscribe(fiber, s => Console.WriteLine(s.ToUpper()));

EventBus<string>.Publish("the message");


//Agents make some things even simpler
var agent = new Agent<string>(s => Console.WriteLine(s.ToUpper()));
agent.Publish("the message");
```

Extras
---------

A separate Fibrous.Extras library provides simple Actors, an IObserver wrapper, subscribable collections and composable pipelines 

```csharp
using var pipeline = 
    Pipeline
        //A stage can take single input and generate an IEnumerable output
        .StartStage<string, string>(Directory.EnumerateFiles) 
        //Ordered fanning out 
        .SelectOrdered(x => (x, new FileInfo(x).Length), 4) 
        // equivalent to Select(x => {f(x); return x;})
        .Tap(info => Console.WriteLine($"**{info.x} is {info.Length} in length")) 
        //Filtering
        .Where(x => x.Length > 1000000)  
        .Tap(info => Console.WriteLine($"{info.x} is {info.Length} in length"));
// the resulting pipeline is still an IStage, and can be composed into another pipeline

pipeline.Publish("C:\\");
```

Proxy
-----

*** Experimental

You can generate a proxy wrapper to a class instance that will take a non-thread safe object and make it thread-safe.

```csharp
    //An interface with only void (regular Fiber) or Task (AsyncFiber proxy) returning methods,
    //and implementing IDisposable
    public interface ITest:IDisposable
    {
        void Init();
        void Add(int i);
        void Subtract(int i);
        event Action<int> Event1;
    }

    //An implementation
    public class Test : ITest
    {
...
    }

    //create a conrete instance
    var t = new Test();

    //Generate a Fiber based proxy (AsyncFiberProxy also available)
    var proxy = FiberProxy<ITest>.Create(t);

    //proxy is now an ITest but wrapped with a Fiber around a concrete ITest instance
    //all methods are wrapped and events are exposed directly.
    
    proxy.Event1 += i => Console.WriteLine(i);

    proxy.Init();
    proxy.Add(1);
    proxy.Subtract(2);

```



	
