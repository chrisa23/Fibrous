using System;
using System.Threading.Tasks;

namespace Fibrous.Pipelines;

/// <summary>
///     Interface for a concurrent processing component.
///     Takes an input and can raise 0+ output events per processing.
///     Exceptions are sent to an error channel
/// </summary>
/// <typeparam name="TIn"></typeparam>
/// <typeparam name="TOut"></typeparam>
public interface IAsyncProcessor<in TIn, out TOut>
{
    event Action<TOut> Output;
    event Action<Exception> Exception;

    /// <summary>
    ///     Main processing call when input received
    /// </summary>
    /// <param name="input"></param>
    Task Process(TIn input);

    /// <summary>
    ///     Allows for any needed initialization, including timer based scheduling to be initialized
    /// </summary>
    /// <param name="scheduler"></param>
    void Initialize(IAsyncScheduler scheduler);
}
