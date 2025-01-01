using System;
using System.Threading.Tasks;

namespace Fibrous;

internal static class ActionConverterUtils
{
#pragma warning disable VSTHRD200
    public static Func<T, Task> ToAsync<T>(this Action<T> action) =>
#pragma warning restore VSTHRD200
        new ActionConverter<T>(action).InvokeAsync;
#pragma warning disable VSTHRD200
    public static Func< Task> ToAsync(this Action action) =>
#pragma warning restore VSTHRD200
        new ActionConverter(action).InvokeAsync;

    private readonly struct ActionConverter(Action action)
    {
        public Task InvokeAsync()
        {
            action.Invoke();
            return Task.CompletedTask;
        }
    }


    private readonly struct ActionConverter<T>(Action<T> action)
    {
        public Task InvokeAsync(T item)
        {
            action.Invoke(item);
            return Task.CompletedTask;
        }
    }
}


