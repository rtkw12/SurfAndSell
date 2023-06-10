using MongoDB.Driver;

namespace Common.Util.Logging;

public interface ILoggingService
{
    bool LogInformation(IClientSessionHandle session, string message, CancellationToken cancellationToken);
    bool LogError(IClientSessionHandle session, string errorMessage, int errorCode, CancellationToken cancellationToken);
    Task<bool> LogInformationAsync(IClientSessionHandle session, string message, CancellationToken cancellationToken);
    Task<bool> LogErrorAsync(IClientSessionHandle session, string errorMessage, int errorCode, CancellationToken cancellationToken);
}