using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UserManagement.API.Interfaces;
using Xunit.Abstractions;

namespace UserManagement.Tests.Infrastructure;

/// <summary>
/// Кастомная фабрика WebApplicationFactory для интеграционного тестирования.
/// Предоставляет:
/// 1. Перенаправление логов ASP.NET Core в xUnit ITestOutputHelper (Доп. задание 3).
/// 2. Изоляцию тестов через сброс in-memory репозитория.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly XUnitLoggerProvider _loggerProvider = new();

    public void SetTestOutputHelper(ITestOutputHelper? outputHelper)
    {
        _loggerProvider.SetOutputHelper(outputHelper);
    }

    /// <summary>
    /// Очищает состояние репозитория для обеспечения полной изоляции тестов.
    /// </summary>
    public void ResetRepository()
    {
        using var scope = Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        repository.Clear();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddProvider(_loggerProvider);
            logging.SetMinimumLevel(LogLevel.Information);
        });

        builder.UseEnvironment("Testing");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _loggerProvider.Dispose();
        }
        base.Dispose(disposing);
    }
}
