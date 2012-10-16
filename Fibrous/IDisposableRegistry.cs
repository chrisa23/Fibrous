using System;

namespace Fibrous
{
    public interface IDisposableRegistry : IDisposable
    {
        void Add(IDisposable toAdd);
        void Remove(IDisposable toRemove);
    }
}