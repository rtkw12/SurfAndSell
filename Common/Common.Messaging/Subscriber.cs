using System.Text;
using Common.Caching.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Common.Caching;

public class Subscriber : MessageBus, ISubscriber
{
    private readonly string _queue;
    private readonly IModel _model;
    private bool _disposed;

    public Subscriber(IMessageConnection messageConnection, 
        string exchangeName, 
        string exchangeType, 
        string queue, 
        string routingKey, 
        int timeToLive = 30000,
        ushort prefetchSize = 10) : base(exchangeName, exchangeType)
    {
        if (messageConnection == null) throw new ArgumentNullException(nameof(messageConnection));
        if (routingKey == null) throw new ArgumentNullException(nameof(routingKey));

        _model = messageConnection.GetConnection().CreateModel();
        _queue = queue ?? throw new ArgumentNullException(nameof(queue));
        var ttl = new Dictionary<string, object>
        {
            {"x-message-ttl", timeToLive }
        };
        _model.ExchangeDeclare(exchangeName, exchangeType, arguments: ttl);
        _model.QueueDeclare(_queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);
        _model.QueueBind(_queue, exchangeName, routingKey);
        _model.BasicQos(0, prefetchSize, false);
    }

    public void Subscribe(Func<string, IDictionary<string, object>, bool> callback)
    {
        var consumer = new EventingBasicConsumer(_model);
        consumer.Received += (sender, e) =>
        {
            var body = e.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            bool success = callback.Invoke(message, e.BasicProperties.Headers);
            if (success)
            {
                _model.BasicAck(e.DeliveryTag, true);
            }
        };

        _model.BasicConsume(_queue, false, consumer);
    }

    public void SubscribeAsync(Func<string, IDictionary<string, object>, Task<bool>> callback)
    {
        var consumer = new AsyncEventingBasicConsumer(_model);
        consumer.Received += async (sender, e) =>
        {
            var body = e.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            bool success = await callback.Invoke(message, e.BasicProperties.Headers);
            if (success)
            {
                _model.BasicAck(e.DeliveryTag, true);
            }
        };

        _model.BasicConsume(_queue, false, consumer);
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