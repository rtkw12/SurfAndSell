using Common.MongoDb;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Common.Util.Logging;

public class LoggingService : MongoService, ILoggingService
{
    private readonly ILogger _logger;

    public LoggingService(IMongoClient client, ILogger logger) : base(client)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private IMongoCollection<LogStorage> Logs() =>
        GetDatabase(DatabaseNames.DATABASE).GetCollection<LogStorage>(DatabaseNames.LOGS);

    public bool LogInformation(IClientSessionHandle session, string message, CancellationToken cancellationToken)
    {
        _logger.LogInformation(message);

        var document = new LogStorage()
        {
            Message = message
        };

        Logs().InsertOne(session, document, cancellationToken: cancellationToken);

        return true;
    }

    public bool LogError(IClientSessionHandle session, string errorMessage, int errorCode, CancellationToken cancellationToken)
    {
        _logger.LogError(errorMessage);

        var document = new LogStorage()
        {
            Message = errorMessage,
            ErrorCode = errorCode
        };

        Logs().InsertOne(session, document, cancellationToken: cancellationToken);

        return true;
    }

    public async Task<bool> LogInformationAsync(IClientSessionHandle session, string message, CancellationToken cancellationToken)
    {
        _logger.LogInformation(message);

        var document = new LogStorage()
        {
            Message = message
        };

        await Logs().InsertOneAsync(session, document, cancellationToken: cancellationToken);

        return true;
    }

    public async Task<bool> LogErrorAsync(IClientSessionHandle session, string errorMessage, int errorCode,
        CancellationToken cancellationToken)
    {
        _logger.LogError(errorMessage);

        var document = new LogStorage()
        {
            Message = errorMessage,
            ErrorCode = errorCode
        };

        await Logs().InsertOneAsync(session, document, cancellationToken: cancellationToken);

        return true;
    }
}