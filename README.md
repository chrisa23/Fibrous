![Banner](Images/Banner.png)

# Fibrous

[![NuGet](https://img.shields.io/nuget/v/Fibrous.svg)](https://www.nuget.org/packages/Fibrous/)

High performance concurrency library for the .NET platform. Fibrous is a fork of
Retlang [http://code.google.com/p/retlang/].

Fibrous is an actor-like framework and also a flexible and pragmatic concurrency toolbox. The main abstractions are
Fibers (execution contexts), Channels (messaging conduits and endpoints), Scheduling, and Events (wrapped,
subscribable .NET events). From these components, you can build simple to complex concurrent libraries and
applications.

Some of the library benefits:

- Tiny library that makes multi-threading easier to reason about
- Thread-safe publishing
- Single or multiple subscribers
- Request/reply
- UI fibers for worry-free UI marshalling
- Batching support
- Scheduling support (cron, `DateTime`, and `TimeSpan` based)

Fibrous is a good fit when you want in-process concurrency with clear ordering semantics without moving to a full
distributed actor framework. It has been used in trading systems, server-side components, and desktop front ends.

If you need distributed concurrency, look into Akka.NET or Proto.Actor. If you need extreme low latency and highly
specialized lock-free structures, look into Disruptor-like approaches instead.

## Packages

- `Fibrous`
  Core library with fibers, channels, request/reply, batching, scheduling, and events.
- `Fibrous.WPF`
  WPF-specific dispatcher fiber and view model helpers.

Install with:

```powershell
dotnet add package Fibrous
dotnet add package Fibrous.WPF
```

## Supported Frameworks

`Fibrous`

- .NET Standard 2.0
- .NET 8
- .NET 10

`Fibrous.WPF`

- .NET 8-windows
- .NET 10-windows

## Core Concepts

### Fibers

Fibers are ordered execution contexts. A single fiber processes one work item at a time, which makes it practical to
keep state behind that fiber without adding your own locking around every mutation.

While a fiber processes sequentially, a system can still use many fibers in parallel and connect them with channels.

Fibers also own child disposables. Subscriptions and timers added through a fiber are cleaned up when the fiber is
disposed.

There are specialized fibers for WPF, which marshal work to the dispatcher thread, and a `StubFiber` for testing and
special cases where inline execution is acceptable.

```csharp
public interface IFiber : IScheduler, IDisposableRegistry
{
    void Enqueue(Action action);
    void Enqueue(Func<Task> action);
}
```

Example:

```csharp
IFiber fiber = new Fiber();

fiber.Enqueue(SomeParameterlessMethod);
fiber.Enqueue(() => DoSomeWork(someParam));

fiber.Schedule(ScheduledMethod, TimeSpan.FromSeconds(5));
fiber.Schedule(ScheduledMethod, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10));
```

### Channels

Channels are the in-memory conduits that implement those ports.

Built-in channel types include:

- `Channel<T>` for normal pub/sub
- `QueueChannel<T>` for work distribution to one of multiple subscribers
- `RequestChannel<TRequest, TReply>` for request/reply
- `SnapshotChannel<T, TSnapshot>` for initial snapshot plus later updates
- `StateChannel<T>` for last-value replay

### Events

Fibrous also includes simple event wrappers and helpers for subscribing fibers to `Action` and `Action<T>` based .NET
events.

## Important Semantics

- Publishing to channels is thread-safe.
- Handlers subscribed through a fiber run on that fiber.
- Ordering is guaranteed per fiber, not across the whole system.
- Disposing a fiber disposes child subscriptions and scheduled work that it owns.
- Batching and last-message subscribers are time-based coordination helpers built on top of fibers and channels.
- Filtered subscribe is publisher-side only for ports that support inline subscription. This avoids queueing discarded
  messages but is a more advanced path than normal fiber subscription.

## Basic Examples

### Channel

```csharp
IFiber fiber = new Fiber();
Channel<string> channel = new();

channel.Subscribe(fiber, msg => Console.WriteLine(msg.ToUpperInvariant()));
channel.Publish("the message");
```

### Create and Subscribe in One Line

```csharp
IFiber fiber = new Fiber();

IChannel<string> channel = fiber.NewChannel<string>(msg => Console.WriteLine(msg.ToUpperInvariant()));
channel.Publish("the message");
```

### Request / Reply

```csharp
IFiber serverFiber = new Fiber();
IFiber clientFiber = new Fiber();

RequestChannel<int, string> channel = new();

channel.SetRequestHandler(serverFiber, request =>
{
    request.Reply($"value:{request.Request}");
    return Task.CompletedTask;
});

channel.SendRequest(42, clientFiber, reply =>
{
    Console.WriteLine(reply);
    return Task.CompletedTask;
});
```

You can also await replies directly:

```csharp
string reply = await channel.SendRequestAsync(42);
Reply<string> timedReply = await channel.SendRequestAsync(42, TimeSpan.FromSeconds(2));
```

### Queue Channel

```csharp
IFiber worker1 = new Fiber();
IFiber worker2 = new Fiber();
QueueChannel<int> queue = new();

queue.Subscribe(worker1, x => Console.WriteLine($"worker1:{x}"));
queue.Subscribe(worker2, x => Console.WriteLine($"worker2:{x}"));

queue.Publish(1);
queue.Publish(2);
queue.Publish(3);
```

Each message is delivered to one subscriber, not all subscribers.

### State Channel

```csharp
StateChannel<string> state = new("starting");
IFiber fiber = new Fiber();

state.Subscribe(fiber, value =>
{
    Console.WriteLine(value);
    return Task.CompletedTask;
});

state.Publish("running");
```

A new subscriber immediately receives the latest value if one exists.

### Snapshot Channel

```csharp
SnapshotChannel<string, string[]> channel = new();
IFiber fiber = new Fiber();

channel.ReplyToPrimingRequest(fiber, () => Task.FromResult(new[] { "a", "b" }));

channel.Subscribe(
    fiber,
    update => Console.WriteLine($"update:{update}"),
    snapshot =>
    {
        Console.WriteLine(string.Join(",", snapshot));
        return Task.CompletedTask;
    });
```

### WPF

`Fibrous.WPF` provides `DispatcherFiber`, which marshals work to the WPF dispatcher thread.

```csharp
DispatcherFiber fiber = new(error => Console.WriteLine(error));

fiber.Enqueue(() =>
{
    StatusText = "Updated on the UI thread";
});
```

## Common Patterns

### Agent

Agents wrap a fiber and a handler to give a lightweight actor-like API.

```csharp
var agent = new Agent<string>(msg =>
{
    Console.WriteLine(msg.ToUpperInvariant());
    return Task.CompletedTask;
}, error => {});

agent.Publish("the message");
```

### Batch and Last

Fibrous has helpers for:

- periodic batches
- keyed periodic batches
- last-message-per-interval delivery

These are useful when UI or downstream systems should not process every individual message immediately.

### Event Bus

`EventBus<T>` gives a static global channel by type.

```csharp
EventBus<string>.Subscribe(fiber, msg => Console.WriteLine(msg));
EventBus<string>.Publish("the message");
```

Use a normal `Channel<T>` when explicit ownership is important.

## Disposal and Lifetime

Most applications should treat fibers as the main ownership boundary.

- create subscriptions and schedules through a fiber
- dispose the fiber when that subsystem is done
- prefer keeping mutable state behind a single owning fiber

Example:

```csharp
using IFiber fiber = new Fiber();
using Channel<int> channel = new();

channel.Subscribe(fiber, x => Console.WriteLine(x));
channel.Publish(1);
```

## Notes on Cron Scheduling

Fibrous includes cron scheduling support without taking an external Quartz package dependency. The cron expression format
follows Quartz-style expressions.

For most scheduling needs, `TimeSpan` and repeated scheduling are simpler and easier to reason about. Use cron when you
really need calendar-style schedules.

## When Fibrous Is a Good Fit

- in-process concurrency
- desktop applications
- message-driven application components
- trading-style shared state and coordination
- replacing ad hoc locking with clearer execution ownership
- mixing scheduling and other concurrency/messaging needs

## When Fibrous Is Probably Not the Right Tool

- distributed actors
- durable workflow/orchestration
- extremely specialized ultra-low-latency systems
- scenarios that need built-in persistence or cluster coordination

## Tests and Examples

The test suite covers the main semantics of ordering, request/reply, batching, state/snapshot behavior, scheduling, and
cleanup. The examples and tests are good places to look when you want to see a particular pattern in use.
