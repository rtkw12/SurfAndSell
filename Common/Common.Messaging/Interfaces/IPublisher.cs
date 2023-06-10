namespace Common.Messaging.Interfaces;

public interface IPublisher : IMessageBus
{
    void Publish<T>(T toPublish, string routingKey, IDictionary<string, object> messageAttributes, string timeToLive = "30000") where T : class;
}