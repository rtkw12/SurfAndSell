using System.Diagnostics.CodeAnalysis;
using System.Text;
using Common.Messaging.Interfaces;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Common.Messaging;

public class Publisher : MessageBus, IPublisher
{
    private readonly IModel _model;
    private bool _disposed;

    public Publisher(IMessageConnection messageConnection, string exchangeName, string exchangeType, int timeToLive = 30000) : base(exchangeName, exchangeType)
    {
        if (messageConnection == null) throw new ArgumentNullException(nameof(messageConnection));

        _model = messageConnection.GetConnection().CreateModel();

        var ttl = new Dictionary<string, object>()
        {
            { "x-message-ttl", timeToLive }
        };
        _model.ExchangeDeclare(exchangeName, exchangeType, arguments: ttl);
    }

    public void Publish<T>([DisallowNull] T toPublish, string routingKey, IDictionary<string, object> messageAttributes, string timeToLive = "30000") where T : class
    {
        if (toPublish == null) throw new ArgumentNullException(nameof(toPublish));
        if (routingKey == null) throw new ArgumentNullException(nameof(routingKey));

        var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(toPublish));

        var properties = _model.CreateBasicProperties();
        properties.Persistent = true;
        properties.Headers = messageAttributes;
        properties.Expiration = timeToLive;

        _model.BasicPublish(ExchangeName, routingKey, properties, body);
    }


    protected override void Dispose(bool dispose)
    {
        if (_disposed)
        {
            return;
        }

        if (dispose)
        {
            _model.Close();
        }

        _disposed = true;
    }
}