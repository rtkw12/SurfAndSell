using Common.Messaging.Interfaces;
using RabbitMQ.Client;

namespace Common.Messaging;

public class MessageConnection : IMessageConnection
{
    private bool _disposed;
    private readonly IConnection _connection;

    public MessageConnection(string url)
    {
        if (url == null) throw new ArgumentNullException(nameof(url));

        var factory = new ConnectionFactory()
        {
            Uri = new Uri(url)
        };
        _connection = factory.CreateConnection();
    }

    public IConnection GetConnection()
    {
        return _connection;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
            _connection?.Close();

        _disposed = true;
    }
}