using System;
using System.Threading.Tasks;
using Fibrous;
using Fibrous.Pipelines;

namespace Example1.ComponentBased;

public class Stage2 : IAsyncProcessor<Payload, Payload>
{
    private readonly ISomeDataAccess _dal;

    public Stage2(ISomeDataAccess service)
    {
        _dal = service;
    }

    public event Action<Payload>   Output;
    public event Action<Exception> Exception;

    public Task Process(Payload input)
    {
        //Do some other things and save to a database
        _dal.SaveData(input);
        Output?.Invoke(input);
        return Task.CompletedTask;
    }

    public void Initialize(IAsyncScheduler scheduler)
    {
        //If we don't need scheduling for this stage, leave empty
    }
}
