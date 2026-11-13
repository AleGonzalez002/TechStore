using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace TechStore.Logging;

public sealed class FileLoggerProvider : ILoggerProvider
{
    private readonly string _directory;
    private readonly ConcurrentDictionary<string, FileLogger> _loggers = new();
    private readonly object _writeLock = new();
    public FileLoggerProvider(string directory) => _directory = directory;
    public ILogger CreateLogger(string categoryName) => _loggers.GetOrAdd(categoryName, name => new FileLogger(name, _directory, _writeLock));
    public void Dispose() { }

    private sealed class FileLogger(string category, string directory, object writeLock) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;
            try
            {
                Directory.CreateDirectory(directory);
                var line = $"{DateTimeOffset.Now:O} [{logLevel}] {category}: {formatter(state, exception)}{(exception is null ? "" : Environment.NewLine + exception)}{Environment.NewLine}";
                lock (writeLock) File.AppendAllText(Path.Combine(directory, $"techstore-{DateTime.Now:yyyyMMdd}.log"), line);
            }
            catch { }
        }
    }
}
