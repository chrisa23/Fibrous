﻿using System;
using Fibrous;

namespace Example1.Pipelines.ComponentBased;

/// <summary>
///     Interface for a concurrent processing component.
///     Takes an input and can raise 0+ output events per processing.
///     Exceptions are sent to an error channel
/// </summary>
/// <typeparam name="TIn"></typeparam>
/// <typeparam name="TOut"></typeparam>
public interface IProcessor<in TIn, out TOut>
{
    event Action<TOut> Output;
    event Action<Exception> Exception;

    /// <summary>
    ///     Main processing call when input received
    /// </summary>
    /// <param name="input"></param>
    void Process(TIn input);

    /// <summary>
    ///     Allows for any needed initialization, including timer based scheduling to be initialized
    /// </summary>
    /// <param name="scheduler"></param>
    void Initialize(IScheduler scheduler);
}
