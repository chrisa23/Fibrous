using System.Threading.Tasks;

namespace Fibrous
{
    /// <summary>
    ///     For use with IEventHub to auto wire events
    ///     Denotes a class which can handle a particular type of message.
    ///     Must be used in conjunction with IHaveAsyncFiber
    /// </summary>
    /// <typeparam name="TMessage">The type of message to handle.</typeparam>
    // ReSharper disable once TypeParameterCanBeVariant
    public interface IHandleAsync<TMessage>
    {
        Task Handle(TMessage message);
    }
}