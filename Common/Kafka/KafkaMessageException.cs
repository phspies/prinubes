using System;
namespace Prinubes.Common.Kafka
{
    [Serializable]
    public class KafkaMessageException : Exception
    {
        public KafkaMessageException() : base() { }
        public KafkaMessageException(string message) : base(message) { }
        public KafkaMessageException(string message, Exception inner) : base(message, inner) { }

        // A constructor is needed for serialization when an
        // exception propagates from a remoting server to the client.
        protected KafkaMessageException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
