Fibrous
=======

High performace concurrency library for the .Net platform.  Fibrous is a fork of Retlang [http://code.google.com/p/retlang/]. 

Fibrous is an actor-like framework that uses the concept of Fibers, Channels and Ports as abstractions.  Fibers represent execution contexts (aka thread/actor/event loop) and Ports represent messaging end points.  

Some of the library benefits:
 - Tiny library that makes multi-threading simple and easy to reason about
 - UI fibers for worry free UI marshalling
 - Excellent batching support
 - UI throttling and redraw can be controlled easily
 - Agent abstractions to simplify working with fibers.
  
 Examples:
	 //create a fiber that is already started and backed by a thread pool
	 IFiber fiber = Fiber.StartNew(FiberType.Pool);
	 
	 //Create a channel and subscribe to messages
	 IChannel<string> channel = new Channel<string>();

	 channel.Subscribe(fiber, (s) => Console.WriteLine(s.ToUpper()));

	 channel.Publish("the message");

	 //You can enqueue methods
	 fiber.Enqueue(SomeParameterlessMethod);
 
	 //or lambdas
	 fiber.Enqueue(() => DoSomeWork(someParam));

	 //You can schedule when things happen
	 fiber.Schedule(ScheduledMethod, when);

	 //and also have them repeat
	 fiber.Schedule(ScheduledMethod, when, repeat);




	