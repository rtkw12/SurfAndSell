namespace Common.Messaging.Interfaces;

public interface ISubscriber : IMessageBus
{
    void Subscribe(Func<string, IDictionary<string, object>, bool> callback);
    void SubscribeAsync(Func<string, IDictionary<string, object>, Task<bool>> callback);
}