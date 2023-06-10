namespace Common.Messaging.Interfaces;

public interface IMessageBus
{
    string ExchangeName { get; }
    string ExchangeType { get; }

    void Dispose();
}