/*using Common.Messaging.Interfaces;
using Common.Messaging.Models;
using JsonConvert = Newtonsoft.Json.JsonConvert;

namespace UserEngine.HostedServices;

public class StoreSubscriber : IHostedService
{
    private readonly ISubscriber _subscriber;
    private readonly IPublisher _publisher;
    private readonly IStoreService _storeService;
    private readonly ILogger _logger;

    public StoreSubscriber(IServiceProvider serviceProvider)
    {
        if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));

        _subscriber = serviceProvider.GetService<ISubscriber>() ?? throw new ArgumentNullException(nameof(_subscriber), "Subscriber cannot be null");
        _publisher = serviceProvider.GetService<IPublisher>() ?? throw new ArgumentNullException(nameof(_publisher), "Publisher cannot be null");
        using var scope = serviceProvider.CreateScope();
        _storeService = scope.ServiceProvider.GetService<IStoreService>() ?? throw new ArgumentNullException(nameof(_storeService), "Store Service cannot be null");
        _logger = scope.ServiceProvider.GetService<ILogger>() ?? throw new InvalidOperationException();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _subscriber.SubscribeResponseAsync(ProcessMessage);
        return Task.CompletedTask;
    }

    private async Task<StoreVerified> ProcessMessage(string message, IDictionary<string, object> headers)
    {
        _logger.LogInformation("Message received from Item Service");
        var itemStore = JsonConvert.DeserializeObject<ItemStoreString>(message);
        using var session = await _storeService.GetSessionAsync();
        var result = await _storeService.TryGetStoreById(session, itemStore?.StoreId ?? throw new InvalidOperationException(), CancellationToken.None);

        return new StoreVerified(itemStore, result.Success);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}*/