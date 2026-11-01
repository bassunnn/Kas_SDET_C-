using System.Net;
using System.Net.Http.Json;
using System.Text;
using UserManagement.API.DTOs;
using UserManagement.API.Models;
using UserManagement.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace UserManagement.Tests.Tests;

/// <summary>
/// Data-Driven тесты для проверки строгой валидации входных данных:
/// - CountryIsoCode: строго 2 буквы латиницы.
/// - Locale: строго формат xx-XX.
/// - UserId: валидный непустой GUID (Guid.Empty запрещен).
/// </summary>
public class UserValidationTests : TestBase
{
    public UserValidationTests(CustomWebApplicationFactory factory, ITestOutputHelper output)
        : base(factory, output)
    {
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("A")] 
    [InlineData("USA")]
    [InlineData("12")] 
    [InlineData("U1")] 
    [InlineData("!$")] 
    [InlineData("U#")] 
    [InlineData("РФ")]
    [InlineData("äö")]
    public async Task Post_WithInvalidCountryIsoCode_ShouldReturnBadRequest(string invalidCountryCode)
    {
        var dto = new UserDto
        {
            CountryIsoCode = invalidCountryCode,
            Locale = "ru-RU"
        };

        var response = await Client.PostAsJsonAsync("/api/users", dto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponseDto>(JsonOptions);
        Assert.NotNull(error);
        Assert.Equal(400, error.Status);
        Assert.NotNull(error.Errors);
        Assert.True(error.Errors.ContainsKey(nameof(UserDto.CountryIsoCode)));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("ru-ru")]
    [InlineData("RU-RU")] 
    [InlineData("RU-ru")]
    [InlineData("ruRU")]
    [InlineData("ru_RU")]
    [InlineData("rus-RU")]
    [InlineData("ru-RUS")]
    [InlineData("12-34")]
    [InlineData("r1-RU")]
    [InlineData("ru-R!")]
    [InlineData("ру-РУ")]
    [InlineData("en-US-extra")]
    public async Task Post_WithInvalidLocale_ShouldReturnBadRequest(string invalidLocale)
    {
        var dto = new UserDto
        {
            CountryIsoCode = "RU",
            Locale = invalidLocale
        };

        var response = await Client.PostAsJsonAsync("/api/users", dto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponseDto>(JsonOptions);
        Assert.NotNull(error);
        Assert.Equal(400, error.Status);
        Assert.NotNull(error.Errors);
        Assert.True(error.Errors.ContainsKey(nameof(UserDto.Locale)));
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")] 
    [InlineData("not-a-guid")] 
    [InlineData("12345")] 
    [InlineData("zzzzzzzz-zzzz-zzzz-zzzz-zzzzzzzzzzzz")]
    public async Task GetById_WithInvalidOrEmptyGuid_ShouldReturnBadRequest(string invalidGuid)
    {
        var response = await Client.GetAsync($"/api/users/{invalidGuid}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponseDto>(JsonOptions);
        Assert.NotNull(error);
        Assert.Equal(400, error.Status);
        Assert.Contains("UserId", error.Detail, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("not-a-valid-guid")]
    public async Task Put_WithInvalidOrEmptyGuid_ShouldReturnBadRequest(string invalidGuid)
    {
        var validDto = new UserDto
        {
            CountryIsoCode = "US",
            Locale = "en-US"
        };

        var response = await Client.PutAsJsonAsync($"/api/users/{invalidGuid}", validDto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponseDto>(JsonOptions);
        Assert.NotNull(error);
        Assert.Equal(400, error.Status);
    }

    [Theory]
    [InlineData("")]
    [InlineData("INVALID")]
    [InlineData("12")]
    public async Task Put_WithInvalidCountryIsoCode_ShouldReturnBadRequest(string invalidCountryCode)
    {
        var createResponse = await Client.PostAsJsonAsync("/api/users", new UserDto
        {
            CountryIsoCode = "FR",
            Locale = "fr-FR"
        });
        var createdUser = await createResponse.Content.ReadFromJsonAsync<User>(JsonOptions);
        Assert.NotNull(createdUser);

        var updateDto = new UserDto
        {
            CountryIsoCode = invalidCountryCode,
            Locale = "fr-FR"
        };

        var response = await Client.PutAsJsonAsync($"/api/users/{createdUser.UserId}", updateDto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponseDto>(JsonOptions);
        Assert.NotNull(error);
        Assert.True(error.Errors?.ContainsKey(nameof(UserDto.CountryIsoCode)));
    }

    [Theory]
    [InlineData("")]
    [InlineData("fr_FR")]
    [InlineData("FR-FR")]
    public async Task Put_WithInvalidLocale_ShouldReturnBadRequest(string invalidLocale)
    {
        var createResponse = await Client.PostAsJsonAsync("/api/users", new UserDto
        {
            CountryIsoCode = "FR",
            Locale = "fr-FR"
        });
        var createdUser = await createResponse.Content.ReadFromJsonAsync<User>(JsonOptions);
        Assert.NotNull(createdUser);

        var updateDto = new UserDto
        {
            CountryIsoCode = "FR",
            Locale = invalidLocale
        };

        var response = await Client.PutAsJsonAsync($"/api/users/{createdUser.UserId}", updateDto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponseDto>(JsonOptions);
        Assert.NotNull(error);
        Assert.True(error.Errors?.ContainsKey(nameof(UserDto.Locale)));
    }

    [Fact]
    public async Task Post_WithEmptyBody_ShouldReturnBadRequest()
    {
        using var emptyContent = new StringContent("", Encoding.UTF8, "application/json");
        var response = await Client.PostAsync("/api/users", emptyContent);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
