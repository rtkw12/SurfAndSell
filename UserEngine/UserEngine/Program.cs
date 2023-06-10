using Common.Caching;
using Common.Messaging;
using Common.Messaging.Interfaces;
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
builder.Services.AddScoped<ISubscriber>(x =>
    new Subscriber(x.GetService<IMessageConnection>() ?? throw new InvalidOperationException(),
        "UserExchange",
        ExchangeType.Topic,
        "User",
        "USER"));

// MongoDB
builder.Services.AddSingleton<IMongoClient>(new MongoClient(builder.Configuration.GetConnectionString("MongoDB")));

// Redis Cache
builder.Services.AddSingleton<ICacheConnectionProvider>(
    new CacheConnectionProvider(builder.Configuration.GetConnectionString("Redis")));
builder.Services.AddSingleton<ICacheService>(c => new CacheService(c.GetService<ICacheConnectionProvider>() ?? throw new InvalidOperationException()));

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
