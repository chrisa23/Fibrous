using System;
using Fibrous;

namespace PipelineExample
{
    public class Channels
    {
        //Channels between stages allow you to tap into the messages flowing 
        //for things like metrics, logging, etc.
        public IChannel<Payload> Input { get; set; } = new Channel<Payload>();
        public IChannel<Payload> Stage1To2 { get; set; } = new Channel<Payload>();
        public IChannel<Payload> Output { get; set; } = new Channel<Payload>();
        public IChannel<Exception> Errors { get; set; } = new Channel<Exception>();
    }
}