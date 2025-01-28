// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System;

namespace Fibrous;

public sealed class Event<TEvent> : IEvent<TEvent>
{
    internal bool HasSubscriptions => InternalEvent != null;

    public IDisposable Subscribe(Action<TEvent> receive)
    {
        InternalEvent += receive;
        return new DisposeAction(() => InternalEvent -= receive);
    }

    public void Publish(TEvent msg)
    {
        Action<TEvent> internalEvent = InternalEvent;
        internalEvent?.Invoke(msg);
    }

    public void Dispose() => InternalEvent = null;

    public IDisposable Subscribe(IObserver<TEvent> observer)
    {
        InternalEvent += observer.OnNext;
        return new DisposeAction(() => InternalEvent -= observer.OnNext);
    }

    private event Action<TEvent> InternalEvent;
}

public sealed class Event : IEvent
{
    public bool HasSubscriptions => InternalEvent != null;

    public IDisposable Subscribe(Action receive)
    {
        InternalEvent += receive;
        return new DisposeAction(() => InternalEvent -= receive);
    }

    public void Trigger()
    {
        Action internalEvent = InternalEvent;
        internalEvent?.Invoke();
    }

    public void Dispose() => InternalEvent = null;

    internal event Action InternalEvent;
}
