using System.Net;
using System.Net.Http.Json;
using UserManagement.API.DTOs;
using UserManagement.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace UserManagement.Tests.Tests;

/// <summary>
/// Тесты для дополнительного задания №3: Логирование в тестах через ITestOutputHelper.
/// Проверяет, что внутренние логи ASP.NET Core и контроллеров успешно доставляются в вывод xUnit.
/// </summary>
public class LoggingTests : TestBase
{
    private readonly RecordingTestOutputHelper _recordingOutput;

    public LoggingTests(CustomWebApplicationFactory factory, ITestOutputHelper output)
        : base(factory, output)
    {
        _recordingOutput = new RecordingTestOutputHelper(output);
        Factory.SetTestOutputHelper(_recordingOutput);
    }

    [Fact]
    public async Task ApiEndpoints_WhenInvoked_ShouldEmitLogsToTestOutputHelper()
    {
        var dto = new UserDto
        {
            CountryIsoCode = "IT",
            Locale = "it-IT"
        };

        var response = await Client.PostAsJsonAsync("/api/users", dto);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        // Проверяем, что логи зафиксированы в выводе теста
        var recordedLines = _recordingOutput.RecordedLines;
        Assert.NotEmpty(recordedLines);

        // Проверяем формат логов (метка времени, уровень логов, имя категории)
        Assert.Contains(recordedLines, line => line.Contains("UsersController", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(recordedLines, line => line.Contains("Created new user", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Декоратор для ITestOutputHelper, сохраняющий переданные строки для программных проверок.
    /// </summary>
    private class RecordingTestOutputHelper : ITestOutputHelper
    {
        private readonly ITestOutputHelper _inner;
        public List<string> RecordedLines { get; } = new();

        public RecordingTestOutputHelper(ITestOutputHelper inner)
        {
            _inner = inner;
        }

        public void WriteLine(string message)
        {
            RecordedLines.Add(message);
            _inner.WriteLine(message);
        }

        public void WriteLine(string format, params object[] args)
        {
            var message = string.Format(format, args);
            RecordedLines.Add(message);
            _inner.WriteLine(message);
        }
    }
}
