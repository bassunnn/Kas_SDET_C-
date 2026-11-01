using System.Net;
using System.Net.Http.Json;
using UserManagement.API.DTOs;
using UserManagement.API.Models;
using UserManagement.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace UserManagement.Tests.Tests;

/// <summary>
/// Тесты основных CRUD операций, проверки 404 Not Found и строгой проверки схемы данных.
/// </summary>
public class UserCrudTests : TestBase
{
    public UserCrudTests(CustomWebApplicationFactory factory, ITestOutputHelper output)
        : base(factory, output)
    {
    }

    [Fact]
    public async Task GetAll_WhenNoUsersExist_ShouldReturnEmptyList()
    {
        var response = await Client.GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var users = await response.Content.ReadFromJsonAsync<List<User>>(JsonOptions);
        Assert.NotNull(users);
        Assert.Empty(users);
    }

    [Fact]
    public async Task GetById_NonExistentGuid_ShouldReturn404NotFound()
    {
        var nonExistentId = Guid.NewGuid();

        var response = await Client.GetAsync($"/api/users/{nonExistentId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponseDto>(JsonOptions);
        Assert.NotNull(error);
        Assert.Equal(404, error.Status);
        Assert.Contains(nonExistentId.ToString(), error.Detail);
    }

    [Fact]
    public async Task Put_NonExistentGuid_ShouldReturn404NotFound()
    {
        var nonExistentId = Guid.NewGuid();
        var updateDto = new UserDto
        {
            CountryIsoCode = "DE",
            Locale = "de-DE"
        };

        var response = await Client.PutAsJsonAsync($"/api/users/{nonExistentId}", updateDto);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponseDto>(JsonOptions);
        Assert.NotNull(error);
        Assert.Equal(404, error.Status);
    }

    [Fact]
    public async Task Post_WithLowercaseCountryIsoCode_ShouldBeStoredAndReturnedInUppercase()
    {
        var dto = new UserDto
        {
            CountryIsoCode = "jp",
            Locale = "ja-JP"
        };

        var response = await Client.PostAsJsonAsync("/api/users", dto);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<User>(JsonOptions);
        Assert.NotNull(created);
        Assert.Equal("JP", created.CountryIsoCode);

        var getResponse = await Client.GetAsync($"/api/users/{created.UserId}");
        var fetched = await getResponse.Content.ReadFromJsonAsync<User>(JsonOptions);
        Assert.NotNull(fetched);
        Assert.Equal("JP", fetched.CountryIsoCode);
    }

    [Fact]
    public async Task Post_ResponseSchemaAndTypes_ShouldMatchSpecification()
    {
        var dto = new UserDto
        {
            CountryIsoCode = "ES",
            Locale = "es-ES"
        };

        var beforePost = DateTime.UtcNow.AddSeconds(-1);
        var response = await Client.PostAsJsonAsync("/api/users", dto);
        var afterPost = DateTime.UtcNow.AddSeconds(1);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<User>(JsonOptions);
        Assert.NotNull(user);

        Assert.NotEqual(Guid.Empty, user.UserId);

        Assert.Equal("ES", user.CountryIsoCode);

        Assert.InRange(user.DateCreated, beforePost, afterPost);

        Assert.Null(user.DateModified);

        Assert.Equal("es-ES", user.Locale);
    }
}
