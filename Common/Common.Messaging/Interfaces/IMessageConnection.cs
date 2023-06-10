using RabbitMQ.Client;

namespace Common.Messaging.Interfaces;

public interface IMessageConnection : IDisposable
{
    IConnection GetConnection();
}