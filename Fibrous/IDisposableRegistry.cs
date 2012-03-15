using System;

namespace Fibrous
{
    public interface IDisposableRegistry : IDisposable
    {
        void Add(IDisposable toAdd);
        bool Remove(IDisposable toRemove);
    }
}