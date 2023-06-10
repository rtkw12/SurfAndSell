namespace Common.Caching.Interfaces;

public interface IMessageBus
{
    string ExchangeName { get; }
    string ExchangeType { get; }

    void Dispose();
}