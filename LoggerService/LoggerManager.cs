using Contracts;
using Microsoft.Extensions.Logging;

namespace LoggerService;

public class LoggerManager : ILoggerManager
{
    private readonly ILogger<LoggerManager> _logger;
    public LoggerManager(ILogger<LoggerManager> logger)
    {
        _logger = logger;
    }
    public void LogDebug(string message, params object[] args) => _logger.LogDebug(message, args);
    public void LogError(string message, params object[] args) => _logger.LogError(message, args);
    public void LogInfo(string message, params object[] args) => _logger.LogInformation(message, args);
    public void LogWarn(string message, params object[] args) => _logger.LogWarning(message, args);
}
