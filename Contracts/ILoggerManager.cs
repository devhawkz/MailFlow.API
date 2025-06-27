

namespace Contracts;

public interface ILoggerManager
{
    void LogDebug(string message, params object[] args);
    void LogInfo(string message, params object[] args);
    void LogWarn(string message, params object[] args);
    void LogError(string message, params object[] args);
}
