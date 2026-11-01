using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using UserManagement.API.DTOs;
using UserManagement.API.Interfaces;
using UserManagement.API.Models;
using UserManagement.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace UserManagement.Tests.Tests;

/// <summary>
/// Тесты для дополнительного задания №2: Глобальная обработка ошибок через Custom Middleware.
/// Проверяет возврат 500 Internal Server Error с чистым JSON и сокрытием внутренних стектрейсов.
/// </summary>
public class ErrorHandlingTests : TestBase
{
    public ErrorHandlingTests(CustomWebApplicationFactory factory, ITestOutputHelper output)
        : base(factory, output)
    {
    }

    [Fact]
    public async Task DiagnosticsEndpoint_WhenUnhandledExceptionOccurs_ShouldReturnClean500Json()
    {
        var response = await Client.GetAsync("/api/diagnostics/throw-unhandled");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var rawBody = await response.Content.ReadAsStringAsync();
        Output.WriteLine($"500 Response Body: {rawBody}");

        var error = await response.Content.ReadFromJsonAsync<ErrorResponseDto>(JsonOptions);
        Assert.NotNull(error);
        Assert.Equal(500, error.Status);
        Assert.Equal("Internal Server Error", error.Title);
        Assert.False(string.IsNullOrWhiteSpace(error.Detail));
        Assert.False(string.IsNullOrWhiteSpace(error.TraceId));

        Assert.DoesNotContain("stacktrace", rawBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("InvalidOperationException", rawBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".cs:", rawBody, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UsersEndpoint_WhenRepositoryFailsUnexpectedly_ShouldReturnClean500Json()
    {
        // Создаем изолированный клиент с репозиторием, выбрасывающим исключение
        var failingClient = Factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<IUserRepository, FailingTestRepository>();
            });
        }).CreateClient();

        var dto = new UserDto
        {
            CountryIsoCode = "US",
            Locale = "en-US"
        };

        var response = await failingClient.PostAsJsonAsync("/api/users", dto);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

        var rawBody = await response.Content.ReadAsStringAsync();
        var error = await response.Content.ReadFromJsonAsync<ErrorResponseDto>(JsonOptions);

        Assert.NotNull(error);
        Assert.Equal(500, error.Status);
        Assert.Equal("Internal Server Error", error.Title);
        Assert.DoesNotContain("Simulated Database Crash", rawBody);
    }

    private class FailingTestRepository : IUserRepository
    {
        public User Add(User user) => throw new System.IO.IOException("Simulated Database Crash");
        public bool Update(User user) => throw new System.IO.IOException("Simulated Database Crash");
        public User? GetById(Guid userId) => throw new System.IO.IOException("Simulated Database Crash");
        public IEnumerable<User> GetAll() => throw new System.IO.IOException("Simulated Database Crash");
        public void Clear() { }
    }
}
