// See https://aka.ms/new-console-template for more information

using Common.Messaging;
using Common.Messaging.Models;
using RabbitMQ.Client;

Console.WriteLine("Started");

var connection = new MessageConnection("amqp://guest:guest@localhost:5672");

var subscriber = new Subscriber(connection, "ItemExchange", ExchangeType.Topic, "Item", "ITEM.*");
subscriber.Subscribe((s, objects) =>
{
    Console.WriteLine(s);

    return true;
});

var publisher = new Publisher(connection, "OrderExchange", ExchangeType.Topic);
publisher.PublishResponse(new ItemStoreString("6485a3c0235a2d9a727b5929", "648584905c15d4acca316d41"), "ITEM.STORE", "ORDER.STORE", null);

Console.ReadKey();