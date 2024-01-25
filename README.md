![Banner](Images/Banner.png)

# Fibrous

[![NuGet](https://img.shields.io/nuget/v/Fibrous.svg)](https://www.nuget.org/packages/Fibrous/)

High performance concurrency library for the .Net platform. Fibrous is a fork of
Retlang [http://code.google.com/p/retlang/].

Fibrous is an actor-like framework but also a flexible and pragmatic concurrency toolbox. The main abstractions are
Fibers, execution contexts, and Channels, messaging channels, but there are many other useful tools and ways to use.

Some of the library benefits:

- Tiny library that makes multi-threading simple and easy to reason about
- Thread safe publishing
- Single or multiple subscribers
- Request reply
- UI fibers for worry free UI marshalling
- Batching support
- Scheduling support (Cron, DateTime and TimeSpan based)

Fibrous is great for multi-threading when you don't need extreme low latency or distributed actors but want an easy to
reason about and extremely flexible messaging based model. Fibrous is also fast. It's in production use in multiple
trading systems, server side and front end.

If you need distributed concurrency, look into Akka.net or Proto.Actor and if you need extreme performance and super low
latency, look into Disruptor.net.

Fibers
------

Fibers are synchronous execution contexts that maintain order of actions. Like Actors, Fibers can manage state without
worries of cross threading issues. While a Fiber processes synchronously, your system can consist of multiple Fibers
communicating through messaging to provide parallelism to your system.

Fibers subscribe to channels to receive messages which queue actions based on the assigned handler. Fibers have a
scheduling API that allows actions to be scheduled in the future as well as repeatedly. You can also directly queue
actions onto a Fiber for it to execute.

Fibers are a repository of IDisposable objects and will dispose of all children upon the Fibers disposal. This is used
to clean up subscriptions and scheduling for any fiber. This is also useful for dealing with children used in the
Fiber's context that implement IDisposable.

There are specialised Fibers for Windows Forms and WPF, which automatically handle invoking actions on the UI/Dispatcher
thread. There is a StubFiber, which is used for testing and special cases, and immediately executes actions on the
calling thread.

```csharp
//Representations of the IFiber interface.
//There are many extensions to enable more complex behavior
public interface IFiber : IDisposable
{
    void Enqueue(Func<Task> action);
    void Enqueue(Action action);
    IDisposable Schedule(Func<Task> action, TimeSpan dueTime);
    IDisposable Schedule(Func<Task> action, TimeSpan startTime, TimeSpan interval);
    IDisposable Schedule(Action action, TimeSpan dueTime);
    IDisposable Schedule(Action action, TimeSpan startTime, TimeSpan interval);
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

The port types (ISubscriberPort<T>, IPublisherPort<T>, IRequestPort<TRequest, TReply>, ISnapshotSubscriberPort<T,
TSnapshot>, IEventPort and IEventTrigger) allow a variety of behavior around message passing and events.

Channels are the conduit for message passing between Fibers, and allow decoupling of the parts of your system. There are
a variety of channel types built into Fibrous: one way channels that notify all subscribers, request/reply, queue
channels that give messages to one of multiple subscribers, as well as a state channel that acts like a last value
cache.

There is a static EventBus which allows a simpler mechanism for passing messages when only one normal channel per type
is needed and an EventHub that allows auto-wiring of handlers.

There are a variety of subscription methods, including filtered, batched, keyed batched and the last message after a
time interval.

Examples:

```csharp
IFiber fiber = new Fiber();

//Create a channel and subscribe to messages
var channel = new Channel<string>();

channel.Subscribe(fiber, s => Console.WriteLine(s.ToUpper()));

//You can also create a channel and subscribe in one line
var channel = fiber.NewChannel<string>(s => Console.WriteLine(s.ToUpper()));

//Publish a message to the channel
channel.Publish("the message");

//EventBus... Global static channel per type (string is only used as an example)
EventBus<string>.Subscribe(fiber, s => Console.WriteLine(s.ToUpper()));

EventBus<string>.Publish("the message");


//Agents make some things even simpler
var agent = new Agent<string>(s => Console.WriteLine(s.ToUpper()));
agent.Publish("the message");
```
