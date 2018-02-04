namespace PipelineExample
{
    using System;
    using Fibrous;
    using Fibrous.Pipeline;

    public class Stage2 : IProcessor<Payload, Payload>
    {
        private readonly ISomeDataAccess _dal;
        public event Action<Payload> Output;
        public event Action<Exception> Exception;

        public Stage2(ISomeDataAccess service)
        {
            _dal = service;
        }

        public void Process(Payload input)
        {
            //Do some other things and save to a database
            _dal.SaveData(input);
            Output?.Invoke(input);
        }

        public void Initialize(IScheduler scheduler)
        {
            //If we don't need scheduling for this stage, leave empty
        }
    }
}