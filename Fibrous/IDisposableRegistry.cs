namespace Fibrous
{
    using System;

    /// <summary>
    /// Collection of disposables, where they can be removed or Disposed together.  
    /// Mostly for internal use, but very convenient for grouping and handling disposables
    /// </summary>
    public interface IDisposableRegistry : IDisposable
    {
        /// <summary>
        /// Add an IDisposable to the registry.  It will be disposed when the registry is disposed.
        /// </summary>
        /// <param name="toAdd"></param>
        void Add(IDisposable toAdd);

        /// <summary>
        /// Remove a disposable from the registry.  It will not be disposed when the registry is disposed.
        /// </summary>
        /// <param name="toRemove"></param>
        void Remove(IDisposable toRemove);
    }
}