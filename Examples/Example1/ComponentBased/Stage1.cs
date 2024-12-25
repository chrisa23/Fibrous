using System;
using System.Threading.Tasks;
using Example1.Pipelines.ComponentBased;
using Fibrous;

namespace Example1.ComponentBased
{
    public class Stage1 : IAsyncProcessor<Payload, Payload>
    {
        private readonly ISomeService _service;

        public Stage1(ISomeService service)
        {
            _service = service;
        }

        public event Action<Payload> Output;
        public event Action<Exception> Exception;

        public Task Process(Payload input)
        {
            //We can have explicit error handling and decide what needs raising or
            //can be handled here or let the ExceptionHandlingExecutor wrap this
            //automatically and raise errors to our error handler for the pipeline
            input.Data["Stage1"] = "Done";
            Output?.Invoke(input);
            return Task.CompletedTask;
        }

        public void Initialize(IScheduler scheduler)
        {
            //We can schedule multiple actions that are all on the same fiber as processing
            //For example, if we have a mapping stage and we need to periodically check for
            //updated mappings
            scheduler.Schedule(UpdateInternalData, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(10));
        }

        private void UpdateInternalData()
        {
            //Once a minute get data we need for this stage (assuming some source that can change, i.e. mappings, state, etc)
            _service.GetLatest();
        }
    }
}
