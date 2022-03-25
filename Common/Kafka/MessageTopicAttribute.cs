namespace Prinubes.Common.Kafka
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MessageTopicAttribute : Attribute
    {
        public MessageTopicAttribute(string topic, int partition)
        {
            Topic = topic;
            Partition = partition;
        }

        public string Topic { get; }
        public int Partition { get; }
    }
}