using System;
using System.Threading.Tasks;

namespace Fibrous;

internal readonly struct ActionConverter
{
    public ActionConverter(Action action)
    {
        _action = action;
    }
    readonly Action _action;
    public Task InvokeAsync()
    {
        _action.Invoke();
        return Task.CompletedTask;
    }
}

