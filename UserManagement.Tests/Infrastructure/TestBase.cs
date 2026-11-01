using System.Net.Http.Json;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace UserManagement.Tests.Infrastructure;

/// <summary>
/// Базовый класс для всех интеграционных тестов.
/// Обеспечивает:
/// - Автоматический сброс репозитория перед каждым тестом.
/// - Подключение ITestOutputHelper для вывода логов запросов в консоль теста (Доп. задание 3).
/// - Настроенный экземпляр HttpClient и параметры JSON сериализации.
/// </summary>
public abstract class TestBase : IClassFixture<CustomWebApplicationFactory>, IDisposable
{
    protected readonly CustomWebApplicationFactory Factory;
    protected readonly ITestOutputHelper Output;
    protected readonly HttpClient Client;

    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    protected TestBase(CustomWebApplicationFactory factory, ITestOutputHelper output)
    {
        Factory = factory;
        Output = output;

        // Связываем логи API с текущим тестовым выводом xUnit
        Factory.SetTestOutputHelper(Output);

        // Гарантируем изоляцию тестов: сброс состояния репозитория перед каждым запуском теста
        Factory.ResetRepository();

        Client = Factory.CreateClient();
    }

    public virtual void Dispose()
    {
        Client.Dispose();
        GC.SuppressFinalize(this);
    }
}
