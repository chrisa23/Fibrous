using System;
using System.Threading.Tasks;
using Fibrous;
using Fibrous.Agents;

namespace Example1
{
    internal class Program
    {
        private static void Main()
        {
            SimpleExample();
            MoreComplexExample();
            AgentExample();
        }

        private static void MoreComplexExample()
        {
            using var calculator = new Calculator();
            calculator.Messages.Publish(new Message(3, Operation.Add));
            calculator.Messages.Publish(new Message(24, Operation.Add));
            calculator.Messages.Publish(new Message(3, Operation.Divide));
            calculator.Messages.Publish(new Message(1.5, Operation.Multiply));

            var result = calculator.Requests.SendRequest(null).Result;
            
            Console.WriteLine(result);
            calculator.Messages.Publish(new Message(0, Operation.Divide));

            Console.ReadKey();
        }

        private static void SimpleExample()
        {
            using var fiber = new Fiber();
            var channel = fiber.NewChannel<string>(Console.WriteLine);

            channel.Publish("Test message");

            var oneSec = TimeSpan.FromSeconds(1);
            fiber.Schedule(() => Console.WriteLine("this was scheduled"), oneSec, oneSec);

            Console.ReadKey();
        }

        private static void AgentExample()
        {
            using IAgent<string> logger = new Agent<string>(Console.WriteLine);
            using IAgent<string> processor = new Agent<string>(s => logger.Publish("Received " + s));
            using var fiber1 = new Fiber();

            var count = 0;
            //Start sending a message after a second, every 2 seconds...
            fiber1.Schedule(() => processor.Publish("Test" + count++), TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2));
            Console.ReadKey();
        }
    }

    public class Calculator : IDisposable
    {
        double _current;
        readonly IFiber _fiber;

        public IChannel<Message> Messages { get; }
        public IRequestPort<object, double> Requests { get; }
        
        public Calculator()
        {
            _fiber = new Fiber(OnError);
            Messages = _fiber.NewChannel<Message>(OnMessage);
            Requests = _fiber.NewRequestPort<object, double>(OnRequest);
        }

        private void OnRequest(IRequest<object, double> obj)
        {
            obj.Reply(_current);
        }

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

        private void OnError(Exception obj)
        {
            Console.WriteLine(obj);
        }

        public void Dispose()
        {
            _fiber?.Dispose();
        }
    }

    public class Message
    {
        public double Value { get; set; }
        public Operation Operation { get; set; }

        public Message(double value, Operation operation)
        {
            Value = value;
            Operation = operation;
        }
    }

    public enum Operation
    {
        Add,
        Subtract,
        Multiply,
        Divide
    }
}
