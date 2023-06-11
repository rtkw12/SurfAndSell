// See https://aka.ms/new-console-template for more information

using Common.Messaging;
using Common.Messaging.Interfaces;
using RabbitMQ.Client;

Console.WriteLine("Started");

var connection = new MessageConnection("amqp://guest:guest@localhost:5672");

var subscriber = new Subscriber(connection,
    "OrderExchange",
    ExchangeType.Topic,
    "Store",
    "ORDER.STORE");

var publisher = new Publisher(connection,
    "ItemExchange",
    ExchangeType.Topic);

subscriber.Subscribe((c, header) =>
{
    Console.WriteLine(c);

    publisher.Publish(true, "ITEM.STORE", null);

    return true;
});


Console.ReadKey();