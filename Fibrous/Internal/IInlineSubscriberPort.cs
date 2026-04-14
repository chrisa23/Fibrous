using System;

namespace Fibrous;

/// <summary>
///     Internal port for publisher-thread subscriptions that bypass fiber enqueueing.
/// </summary>
internal interface IInlineSubscriberPort<out T>
{
    IDisposable SubscribeInline(Action<T> receive);
}
