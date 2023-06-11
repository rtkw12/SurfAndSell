namespace Common.Messaging.Interfaces;

public interface ISubscriber : IMessageBus
{
    void Subscribe(Func<string, IDictionary<string, object>, bool> callback);
    void SubscribeAsync(Func<string, IDictionary<string, object>, Task<bool>> callback);
    void SubscribeResponse<T>(Func<string, IDictionary<string, object>, T> callback);
    void SubscribeResponseAsync<T>(Func<string, IDictionary<string, object>, Task<T>> callback);
}