namespace Examples
{
    using System;
    using System.Collections.Generic;
    using Fibrous;
    using Fibrous.Actors;
    using Fibrous.Channels;
    using Fibrous.Fibers;

    /// <summary>
    /// Very simple, non-functioning, system example.
    /// </summary>
    public class Channels
    {
        public readonly IChannel<object> SystemIn = new Channel<object>();
        public readonly IChannel<string> LogsOut = new Channel<string>();
        public readonly IChannel<double> DataOut = new Channel<double>();
    }

    public class BusinessLogic
    {
        //work going on and outputing messages or information
        private readonly IFiber _fiber;
        private readonly Channels _channels;

        public BusinessLogic(IFiber fiber, Channels channels)
        {
            _fiber = fiber;
            _channels = channels;
            _channels.SystemIn.Subscribe(_fiber, OnMsg);
        }

        private void OnMsg(object obj)
        {
        }
    }

    public class FileLogging : IDisposable
    {
        private readonly BatchingActor<string> _actor;
        private readonly string _path;

        public FileLogging(ISubscribePort<string> input, string path)
        {
            _path = path;
            input.Subscribe(new StubFiber(), x => _actor.Publish(x));
            _actor = BatchingActor<string>.Start(x => LogToFile(_path, x), new TimeSpan(0, 0, 1));
        }

        private static void LogToFile(string path, IList<string> items)
        {
            //log the lines to file... File.AppendLines
        }

        public void Dispose()
        {
            _actor.Dispose();
        }
    }

    public class UIDisplay
    {
        private readonly IFiber _fiber;
        private readonly ISubscribePort<double> _input;

        public UIDisplay(ISubscribePort<double> input)
        {
            _fiber = ThreadFiber.StartNew(); //would be FormFiber or Dispatcher fiber
            _input = input;
            _input.SubscribeToBatch(_fiber, OnBatch, new TimeSpan(0, 0, 0, 0, 100));
        }

        private void OnBatch(double[] obj)
        {
        }
    }

    public class SystemExample
    {
        private readonly Channels _channels = new Channels();
        private readonly IFiber _blFiber;
        private BusinessLogic _bl;
        private FileLogging _logging;
        private UIDisplay _display;

        public SystemExample()
        {
            _blFiber = ThreadFiber.StartNew();
            _bl = new BusinessLogic(_blFiber, _channels);
            _logging = new FileLogging(_channels.LogsOut, ".");
            _display = new UIDisplay(_channels.DataOut);
            //dispose of everything when done...
        }
    }
}