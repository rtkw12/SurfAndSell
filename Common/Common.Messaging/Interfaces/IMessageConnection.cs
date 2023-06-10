using RabbitMQ.Client;

namespace Common.Caching.Interfaces;

public interface IMessageConnection : IDisposable
{
    IConnection GetConnection();
}