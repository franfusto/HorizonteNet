using Microsoft.Extensions.Logging;

namespace Horizonte;

public class ExistingLoggerProvider : ILoggerProvider
{
    private readonly ILogger _logger;
    public ExistingLoggerProvider(ILogger logger) => _logger = logger;
    public ILogger CreateLogger(string categoryName) => _logger;
    public void Dispose() { }
}