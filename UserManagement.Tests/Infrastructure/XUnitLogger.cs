using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace UserManagement.Tests.Infrastructure;

/// <summary>
/// Провайдер логирования для перенаправления логов ASP.NET Core в xUnit ITestOutputHelper.
/// Позволяет выводить логи API в консоль теста / отчеты CI при локальном запуске и в CI.
/// </summary>
public class XUnitLoggerProvider : ILoggerProvider
{
    private ITestOutputHelper? _outputHelper;

    public XUnitLoggerProvider(ITestOutputHelper? outputHelper = null)
    {
        _outputHelper = outputHelper;
    }

    public void SetOutputHelper(ITestOutputHelper? outputHelper)
    {
        _outputHelper = outputHelper;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new XUnitLogger(categoryName, () => _outputHelper);
    }

    public void Dispose()
    {
        _outputHelper = null;
        GC.SuppressFinalize(this);
    }
}

public class XUnitLogger : ILogger
{
    private readonly string _categoryName;
    private readonly Func<ITestOutputHelper?> _outputHelperAccessor;

    public XUnitLogger(string categoryName, Func<ITestOutputHelper?> outputHelperAccessor)
    {
        _categoryName = categoryName;
        _outputHelperAccessor = outputHelperAccessor;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        var output = _outputHelperAccessor();
        if (output is null)
        {
            return;
        }

        try
        {
            var message = formatter(state, exception);
            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var line = $"[{timestamp}] [{logLevel,-11}] [{_categoryName}] {message}";

            if (exception is not null)
            {
                line += $"{Environment.NewLine}{exception}";
            }

            output.WriteLine(line);
        }
        catch
        {
            // Игнорируем исключения, если xUnit test runner уже завершил выполнение теста
        }
    }
}
