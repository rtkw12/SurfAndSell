using Common.Caching;
using Common.Messaging;
using Common.Messaging.Interfaces;
using Common.Util.Logging;
using ItemEngine;
using ItemEngine.HostedService;
using ItemEngine.Runtime;
using MongoDB.Driver;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// RabbitMQ
builder.Services.AddSingleton<IMessageConnection>(
    new MessageConnection(builder.Configuration.GetConnectionString("RabbitMQ")));
builder.Services.AddSingleton<ISubscriber>(x =>
    new Subscriber(x.GetService<IMessageConnection>() ?? throw new InvalidOperationException(),
        "ItemExchange",
        ExchangeType.Topic,
        "Item",
        "ITEM.*"));
builder.Services.AddSingleton<IPublisher>(x =>
    new Publisher(x.GetService<IMessageConnection>() ?? throw new InvalidOperationException(),
        "OrderExchange",
        ExchangeType.Topic));

// MongoDB
builder.Services.AddSingleton<IMongoClient>(new MongoClient(builder.Configuration.GetConnectionString("MongoDB")));

// Redis Cache
builder.Services.AddSingleton<ICacheConnectionProvider>(
    new CacheConnectionProvider(builder.Configuration.GetConnectionString("Redis")));
builder.Services.AddSingleton<ICacheService>(c => new CacheService(c.GetService<ICacheConnectionProvider>() ?? throw new InvalidOperationException()));

// Logging
builder.Services.AddLogging();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
var logger = builder.Services.BuildServiceProvider().GetService<ILogger<LoggingService>>();
builder.Services.AddSingleton(typeof(ILogger), logger ?? throw new InvalidOperationException());
builder.Services.AddScoped<ILoggingService, LoggingService>();

// Services
builder.Services.AddScoped<IItemService, ItemService>();

// Hosted Services
builder.Services.AddHostedService<ItemStoreListener>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
