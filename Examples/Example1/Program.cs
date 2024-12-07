using System;
using System.Threading.Tasks;
using Fibrous;
using Fibrous.Agents;

namespace Example1
{
    internal class Program
    {
        private static async Task Main()
        {
            ExceptionTest();

            SimpleExample();
            await MoreComplexExample();
            AgentExample();
        }

        private static void ExceptionTest()
        {
            //Exceptions get lost with AsyncFiber, so use an
            //exception callback
            Fiber h = new Fiber();
            for (int i = 0; i < 10; i++)
            {
                h.Enqueue(() =>
                {
                    Console.WriteLine("Test");
                    throw new Exception();
                });
            }

            Console.ReadKey();
        }

        private static async Task MoreComplexExample()
        {
            using Calculator calculator = new Calculator();
            calculator.Messages.Publish(new Message(3, Operation.Add));
            calculator.Messages.Publish(new Message(24, Operation.Add));
            calculator.Messages.Publish(new Message(3, Operation.Divide));
            calculator.Messages.Publish(new Message(1.5, Operation.Multiply));

            double result = await calculator.Requests.SendRequestAsync(new object());

            Console.WriteLine(result);
            calculator.Messages.Publish(new Message(0, Operation.Divide));

            Console.ReadKey();
        }

        private static void SimpleExample()
        {
            using Fiber fiber   = new Fiber();
            IChannel<string> channel = fiber.NewChannel<string>(Console.WriteLine);

            channel.Publish("Test message");

            TimeSpan oneSec = TimeSpan.FromSeconds(1);
            fiber.Schedule(() => Console.WriteLine("this was scheduled"), oneSec, oneSec);

            Console.ReadKey();
        }

        private static void AgentExample()
        {
            //agents provide thread safety to individual functions.
            //in this case, we want one point for storing to the database that can be shared
            using Agent<object> dataAccess = new Agent<object>(StoreToDatabase, x => { });

            object latestData = new object();

            //this could be called from multiple places, but all saves are sequential and thread safe
            dataAccess.Publish(latestData);

            Console.ReadKey();
        }

        private static async Task StoreToDatabase(object toStore)
        {
            //Simulate storing data to a db
            Console.WriteLine("Storing to database");
            await Task.Delay(1000);
        }
    }

    public class Calculator : IDisposable
    {
        private readonly IFiber _fiber;
        private double _current;

        public Calculator()
        {
            _fiber = new Fiber(OnError);
            Messages = _fiber.NewChannel<Message>(OnMessage);
            Requests = _fiber.NewRequestPort<object, double>(OnRequest);
        }

        public IPublisherPort<Message> Messages { get; }
        public IRequestPort<object, double> Requests { get; }

        public void Dispose() => _fiber?.Dispose();

        private async Task OnRequest(IRequest<object, double> obj) => obj.Reply(_current);

        private void OnMessage(Message obj)
        {
            switch (obj.Operation)
            {
                case Operation.Add:
                    _current += obj.Value;
                    break;
                case Operation.Subtract:
                    _current -= obj.Value;
                    break;
                case Operation.Multiply:
                    _current *= obj.Value;
                    break;
                case Operation.Divide:
                    _current /= obj.Value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnError(Exception obj) => Console.WriteLine(obj);
    }

    public class Message
    {
        public Message(double value, Operation operation)
        {
            Value = value;
            Operation = operation;
        }

        public double Value { get; set; }
        public Operation Operation { get; set; }
    }

    public enum Operation
    {
        Add,
        Subtract,
        Multiply,
        Divide
    }
}
