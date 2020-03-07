Fibrous
=======

High performance concurrency library for the .Net platform.  Fibrous is a fork of Retlang [http://code.google.com/p/retlang/]. 

Fibrous is an actor-like framework but also a flexible set of concurrency components. The main abstractions are Fibers, Ports and Channels.  Fibers are execution contexts, Ports are messaging end points, and channels are combinations of ports that allow decoupling of fiber components.

Some of the library benefits:
 - Tiny library that makes multi-threading simple and easy to reason about
 - Thread safe publishing
 - Single or multiple subscribers
 - Request reply
 - UI fibers for worry free UI marshalling
 - Batching support
 - Scheduling support (Cron, DateTime and TimeSpan based)
 - Easy Pipeline construction
 
 Fibrous is great for multi-threading when you don't need extreme low latency or distributed actors but want an easy to reason about messaging based model.  Fibrous is also fast.

 If you need distributed concurrency, look into Akka.net or Proto.Actor.  If you need extreme performance and super low latency, look into Disruptor.net.

Fibers
------

Fibers are synchronous execution contexts that maintain order of actions.  Like Actors, Fibers can manage state without worries of cross threading issues.  While a Fiber is synchronous, your system can consist of multiple Fibers communicating through messaging to provide parallelism to your system.

Fibers subscribe to channels to receive messages which queue actions based on the assigned handler.  Fibers have a scheduling API that allows actions to be scheduled in the future as well as repeatedly.  You can also directly queue actions onto a Fiber for it to execute.

Fibers are a repository of IDisposable objects and will dispose of all children upon the Fibers disposal.  This is used to clean up subscriptions and scheduling for any fiber.  This is also useful for dealing with children used in the Fiber's context that implement IDisposable.

There are specialised Fibers for Windows Forms and WPF, which automatically handle invoking actions on the UI/Dispatcher thread.  There is a StubFiber, which is used for testing and in special cases, and immediately executes actions on the calling thread.

```csharp
//Representation of the main IFiber interface.
//There are many extensions to enable more complex behavior
public interface IFiber : IDisposable
{
    void Enqueue(Action action);
    IDisposable Schedule(Action action, TimeSpan dueTime);
    IDisposable Schedule(Action action, TimeSpan startTime, TimeSpan interval);
    void Add(IDisposable toAdd);
    void Remove(IDisposable toRemove);
}
```

Ports and Channels
------------------

Ports are the end points for publishing and subscribing to messages.  

Channels are the conduit for message passing between Fibers, and allow decoupling of the parts of your system.  There are a variety of channel types built into Fibrous: one way channels that notify all subscribers, request/reply, queue channels that give messages to one of multiple subscribers, as well as a state channel that acts like a last value cache.

There is a static EventBus which allows a simpler mechanism for passing messages when only one normal channel per type is needed and an EventHub that allows auto-wiring of handlers.

There are a variety of subscription methods, including filtered, batched, keyed batched and the last message after a time interval.
 
  
Examples:

```csharp
//Work is done on the thread pool, but in a sequential fashion 
IFiber fiber = new Fiber();  //this is v4 syntax where Start/Stop is no longer part of the API
	 
//Create a channel and subscribe to messages
IChannel<string> channel = new Channel<string>();

channel.Subscribe(fiber, s => Console.WriteLine(s.ToUpper()));

//You can also create a channel and subscribe in one line
IChannel<string> channel = fiber.NewChannel<string>(s => Console.WriteLine(s.ToUpper()));

//Publish a message to the channel
channel.Publish("the message");

//You can enqueue methods
fiber.Enqueue(SomeParameterlessMethod);
 
//or lambdas
fiber.Enqueue(() => DoSomeWork(someParam));

//You can schedule when things happen
fiber.Schedule(ScheduledMethod, when);

//and also have them repeat
fiber.Schedule(ScheduledMethod, startWhen, repeatInterval);

//Agents make some things even simpler
var agent = new Agent<string>(s => Console.WriteLine(s.ToUpper()));
agent.Publish("the message");
```

Pipelines
---------

-- TBD --

```csharp
using var pipeline = 
    new Stage<string, string>(Directory.EnumerateFiles) //A stage can take single input and generate an IEnumerable output
    .SelectOrdered(x => (x, new FileInfo(x).Length), 4) //Ordered fanning out 
    .Tap(info => Console.WriteLine($"**{info.x} is {info.Length} in length")) // equivalent to Select(x => {f(x); return x;})
    .Where(x => x.Length > 1000000)  //Filtering
    .Tap(info => Console.WriteLine($"{info.x} is {info.Length} in length"));
// the resulting pipeline is still an IStage, and can be composed into another pipeline

pipeline.Publish("C:\\");
```

How to Use
----------

There are a variety of patterns for using Fibrous.  

One of the simplest is to use an Agent as a thread safe action queue.  You can use this for things like asynchronous logging or storing data to a database.

Extras
------

There are a variety of extra features like Fiber backed collections, Pipelines, Agents and Actors.

	