using Common.Messaging.Interfaces;
using Common.Messaging.Models;
using ItemEngine.Models;
using Newtonsoft.Json;

namespace ItemEngine.HostedService;

public class ItemStoreListener : IHostedService
{
    private readonly ISubscriber _subscriber;
    private readonly IItemService _itemService;
    private readonly ILogger _logger;

    public ItemStoreListener(IServiceProvider serviceProvider)
    {
        if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));

        _subscriber = serviceProvider.GetService<ISubscriber>() ?? throw new InvalidOperationException();
        using var scope = serviceProvider.CreateScope();
        _itemService = scope.ServiceProvider.GetService<IItemService>() ?? throw new InvalidOperationException();
        _logger = scope.ServiceProvider.GetService<ILogger>() ?? throw new InvalidOperationException();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _subscriber.SubscribeAsync(ProcessMessage);
        return Task.CompletedTask;
    }

    public async Task<bool> ProcessMessage(string message, IDictionary<string, object> headers)
    {
        if (message.Contains("Store"))
        {
            using var session = await _itemService.GetSessionAsync();
            try
            {
                var itemVerified = JsonConvert.DeserializeObject<StoreVerified>(message);
                if (itemVerified == null)
                {
                    throw new Exception($"Item cannot be resolved to type '{typeof(StoreVerified)}'");
                }
                return await _itemService.UpdateItemVerification(session, itemVerified.StoreId, itemVerified.ItemId,
                    CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        return true;
    }
    
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}