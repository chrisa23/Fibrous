namespace Fibrous
{
    using System;

    public interface IDisposableRegistry : IDisposable
    {
        void Add(IDisposable toAdd);
        void Remove(IDisposable toRemove);
    }
}