Fibrous
=======

High performance concurrency library for the .Net platform.  Fibrous is a fork of Retlang [http://code.google.com/p/retlang/]. 

Fibrous is an actor-like framework but also a flexible set of concurrency components. The main abstractions are Fibers, Ports and Channels.  Fibers are execution contexts, Ports are messaging end points, and channels are combinations of ports that allow decoupling of fiber components.

Some of the library benefits:
 - Tiny library that makes multi-threading simple and easy to reason about
 - Thread safe publishing
 - Single or multiple subscribers
 - UI fibers for worry free UI marshalling
 - Batching support
 - Scheduling support (Cron, DateTime and TimeSpan based)
 - Synchronous and asynchronous request reply
 - Easy Pipeline construction
 
 Fibrous is great for multi-threading when you don't need extreme low latency or distributed actors but want an easy to reason about messaging based model.  Fibrous is also fast.

 If you need distributed concurrency, look into Akka.net or Proto.Actor.  If you need extreme performance and super low latency, look into Disruptor.net.

Fibers
------

Fibers are synchronous execution contexts that maintain order of actions.  Like Actorx, Fibers can manage state without worries of cross threading issues.  While a Fiber is synchronous, your system can consist of multiple Fibers communicating through messaging to provide parallelism to your system.

Fibers subscribe to channels to receive messages which queue actions based on the assigned handler.  Fibers have a scheduling API that allows actions to be scheduled in the future as well as repeatedly.  You can also directly queue actions onto a Fiber for it to execute.

Fibers are a repository of IDisposable objects and will dispose of all children upon the Fibers disposal.  This is used to clean up subscriptions and scheduling for any fiber.  This is also useful for dealing with children used in the Fiber's context that implement IDisposable.

There are specialised Fibers for Windows Forms and WPF, which automatically handle invoking actions on the UI/Dispatcher thread.  There is a StubFiber, which is used for testing and in special cases, and immediately executes actions on the calling thread.

Ports and Channels
------------------

Ports are the end points for publishing and subscribing to messages.  

Channels are the conduit for message passing between Fibers, and allow decoupling of the parts of your system.  There are a variety of channel types built into Fibrous: one way channels that notify all subscribers, request/reply, queue channels that give messages to one of multiple subscribers, as well as a state channel that acts like a last value cache.

There is a static EventBus which allows a simpler mechanism for passing messages when only one normal channel per type is needed and and EventHub that allows auto-wiring of handlers.

There are a variety of subscription methods, including filtered, batched, keyed batched and the last message after a time interval.
 
  
Examples:

```
//Work is done on the thread pool, but in a sequential fashion 
IFiber fiber = new Fiber();
	 
//Create a channel and subscribe to messages
IChannel<string> channel = new Channel<string>();

channel.Subscribe(fiber, s => Console.WriteLine(s.ToUpper()));

// or you can subscribe via the Fiber
fiber.Subscribe(channel, s => Console.WriteLine(s.ToUpper()));

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

Extras
------

There are a variety of extra features like Fiber backed collections, Pipelines, Agents and Actors.

	