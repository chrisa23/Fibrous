using System;

namespace Fibrous;

internal interface IInlineSubscriberPort<out T>
{
    IDisposable SubscribeInline(Action<T> receive);
}
