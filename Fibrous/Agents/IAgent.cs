﻿namespace Fibrous
{
    using System;
    /// <summary>
    /// Actor like abstraction
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IAgent<T> : IPublisherPort<T>, IDisposableRegistry
    {
    }
    
    public abstract class AgentBase<T> : IAgent<T>
    {
        private readonly IChannel<T> _channel = new Channel<T>();
        private readonly IFiber _fiber;

        //can't bacth....  
        protected AgentBase(FiberType type = FiberType.Pool)
        {
            _fiber = Fiber.StartNew(type);
            _channel.Subscribe(_fiber, Handler);
        }

        public bool Publish(T msg)
        {
            return _channel.Publish(msg);
        }

        public abstract void Handler(T msg);

        public void Dispose()
        {
            _fiber.Dispose();
        }

        public void Add(IDisposable toAdd)
        {
            _fiber.Add(toAdd);
        }

        public void Remove(IDisposable toRemove)
        {
            _fiber.Remove(toRemove);
        }
    }

    public class Agent<T> : AgentBase<T>
    {
        private readonly Action<T> _handler;

        public Agent(Action<T> handler, FiberType type = FiberType.Pool) : base(type)
        {
            _handler = handler;
        }

        public override void Handler(T msg)
        {
            _handler(msg);
        }

        public static IAgent<T> Start(Action<T> handler, FiberType type = FiberType.Pool)
        {
            return new Agent<T>(handler, type);
        }
    }
}