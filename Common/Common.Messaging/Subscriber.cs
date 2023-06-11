using System.Text;
using Common.Messaging.Interfaces;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Common.Messaging;

public class Subscriber : MessageBus, ISubscriber
{
    private readonly string _queue;
    private readonly string? _replyExchangeName;
    private readonly IModel _model;
    private bool _disposed;

    public Subscriber(IMessageConnection messageConnection, 
        string exchangeName, 
        string exchangeType, 
        string queue, 
        string routingKey, 
        string? replyExchangeName = null,
        int timeToLive = 30000,
        ushort prefetchSize = 10) : base(exchangeName, exchangeType)
    {
        if (messageConnection == null) throw new ArgumentNullException(nameof(messageConnection));
        if (routingKey == null) throw new ArgumentNullException(nameof(routingKey));

        _model = messageConnection.GetConnection().CreateModel();
        _queue = queue ?? throw new ArgumentNullException(nameof(queue));
        _replyExchangeName = replyExchangeName;
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
        consumer.Received += (model, eventArgs) =>
        {
            var body = eventArgs.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            bool success = callback.Invoke(message, eventArgs.BasicProperties.Headers);
            if (success)
            {
                _model.BasicAck(eventArgs.DeliveryTag, true);
            }
        };

        _model.BasicConsume(_queue, true, consumer);
    }

    public void SubscribeAsync(Func<string, IDictionary<string, object>, Task<bool>> callback)
    {
        var consumer = new AsyncEventingBasicConsumer(_model);
        consumer.Received += async (model, eventArgs) =>
        {
            var body = eventArgs.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            bool success = await callback.Invoke(message, eventArgs.BasicProperties.Headers);
            if (success)
            {
                _model.BasicAck(eventArgs.DeliveryTag, true);
            }
        };

        _model.BasicConsume(_queue, true, consumer);
    }

    public void SubscribeResponse<T>(Func<string, IDictionary<string, object>, T> callback)
    {
        var consumer = new EventingBasicConsumer(_model);
        consumer.Received += async (model, eventArgs) =>
        {
            var body = eventArgs.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var properties = eventArgs.BasicProperties;

            var replyProps = _model.CreateBasicProperties();
            replyProps.CorrelationId = properties.CorrelationId;

            var response = callback.Invoke(message, eventArgs.BasicProperties.Headers);

            var replyMessage = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response));
            _model.BasicPublish(_replyExchangeName, eventArgs.BasicProperties.ReplyTo, replyProps , replyMessage);
        };

        _model.BasicConsume(_queue, true, consumer);
    }

    public void SubscribeResponseAsync<T>(Func<string, IDictionary<string, object>, Task<T>> callback)
    {
        var consumer = new EventingBasicConsumer(_model);
        consumer.Received += async (model, eventArgs) =>
        {
            var body = eventArgs.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var properties = eventArgs.BasicProperties;

            var replyProps = _model.CreateBasicProperties();
            replyProps.CorrelationId = properties.CorrelationId;

            var response = await callback.Invoke(message, eventArgs.BasicProperties.Headers);

            var replyMessage = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response));
            _model.BasicPublish(_replyExchangeName, eventArgs.BasicProperties.ReplyTo, replyProps, replyMessage);
        };

        _model.BasicConsume(_queue, true, consumer);
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