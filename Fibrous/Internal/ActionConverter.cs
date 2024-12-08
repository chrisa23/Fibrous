using System;
using System.Threading.Tasks;

namespace Fibrous;

internal readonly struct ActionConverter(Action action)
{
    public Task InvokeAsync()
    {
        action.Invoke();
        return Task.CompletedTask;
    }
}

