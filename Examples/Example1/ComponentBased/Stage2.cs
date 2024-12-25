using System;
using System.Threading.Tasks;
using Example1.Pipelines.ComponentBased;
using Fibrous;

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

    public void Initialize(IScheduler scheduler)
    {
        //If we don't need scheduling for this stage, leave empty
    }
}
