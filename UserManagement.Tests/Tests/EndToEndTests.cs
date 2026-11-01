using System.Net;
using System.Net.Http.Json;
using UserManagement.API.DTOs;
using UserManagement.API.Models;
using UserManagement.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace UserManagement.Tests.Tests;

/// <summary>
/// Обязательный сквозной сценарий тестирования (End-to-End):
/// Создание через POST -> Получение через GET по ID -> Модификация через PUT -> Проверка изменений через повторный GET.
/// </summary>
public class EndToEndTests : TestBase
{
    public EndToEndTests(CustomWebApplicationFactory factory, ITestOutputHelper output)
        : base(factory, output)
    {
    }

    [Fact]
    public async Task FullUserLifecycle_CreateGetUpdateGet_ShouldSucceedWithCorrectSchemaAndState()
    {

        // Создание пользователя через POST /api/users
        var createDto = new UserDto
        {
            CountryIsoCode = "us", // проверяем приведение к UPPERCASE
            Locale = "en-US"
        };

        var postResponse = await Client.PostAsJsonAsync("/api/users", createDto);
        Output.WriteLine($"POST Response Status: {postResponse.StatusCode}");

        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        Assert.NotNull(postResponse.Headers.Location);

        var createdUser = await postResponse.Content.ReadFromJsonAsync<User>(JsonOptions);
        Assert.NotNull(createdUser);

        // Проверка схемы и типов
        Assert.NotEqual(Guid.Empty, createdUser.UserId);
        Assert.Equal("US", createdUser.CountryIsoCode);
        Assert.Equal("en-US", createdUser.Locale);
        Assert.Null(createdUser.DateModified);
        Assert.True(createdUser.DateCreated <= DateTime.UtcNow);
        Assert.True(createdUser.DateCreated > DateTime.UtcNow.AddMinutes(-1));

        var expectedLocationUri = $"/api/Users/{createdUser.UserId}";
        Assert.EndsWith(createdUser.UserId.ToString(), postResponse.Headers.Location.ToString(), StringComparison.OrdinalIgnoreCase);

        // Получение созданного пользователя через GET /api/users/{userId}
        var getByIdResponse = await Client.GetAsync($"/api/users/{createdUser.UserId}");
        Output.WriteLine($"GET by ID Response Status: {getByIdResponse.StatusCode}");

        Assert.Equal(HttpStatusCode.OK, getByIdResponse.StatusCode);
        var retrievedUser = await getByIdResponse.Content.ReadFromJsonAsync<User>(JsonOptions);
        Assert.NotNull(retrievedUser);

        Assert.Equal(createdUser.UserId, retrievedUser.UserId);
        Assert.Equal("US", retrievedUser.CountryIsoCode);
        Assert.Equal("en-US", retrievedUser.Locale);
        Assert.Equal(createdUser.DateCreated, retrievedUser.DateCreated);
        Assert.Null(retrievedUser.DateModified);

        // Модификация пользователя через PUT /api/users/{userId}
        // Небольшая задержка, чтобы DateModified строго отличался от DateCreated
        await Task.Delay(20);

        var updateDto = new UserDto
        {
            CountryIsoCode = "gb",
            Locale = "en-GB"
        };

        var putResponse = await Client.PutAsJsonAsync($"/api/users/{createdUser.UserId}", updateDto);
        Output.WriteLine($"PUT Response Status: {putResponse.StatusCode}");

        Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);
        var updatedUser = await putResponse.Content.ReadFromJsonAsync<User>(JsonOptions);
        Assert.NotNull(updatedUser);

        Assert.Equal(createdUser.UserId, updatedUser.UserId);
        Assert.Equal("GB", updatedUser.CountryIsoCode);
        Assert.Equal("en-GB", updatedUser.Locale);
        Assert.Equal(createdUser.DateCreated, updatedUser.DateCreated);
        Assert.NotNull(updatedUser.DateModified);
        Assert.True(updatedUser.DateModified.Value >= createdUser.DateCreated);

        // Проверка изменений через повторный GET /api/users/{userId}
        var getAfterUpdateResponse = await Client.GetAsync($"/api/users/{createdUser.UserId}");
        Assert.Equal(HttpStatusCode.OK, getAfterUpdateResponse.StatusCode);

        var finalUser = await getAfterUpdateResponse.Content.ReadFromJsonAsync<User>(JsonOptions);
        Assert.NotNull(finalUser);
        Assert.Equal("GB", finalUser.CountryIsoCode);
        Assert.Equal("en-GB", finalUser.Locale);
        Assert.NotNull(finalUser.DateModified);
        Assert.Equal(updatedUser.DateModified, finalUser.DateModified);

        // Проверка присутствия пользователя в общем списке GET /api/users
        var getAllResponse = await Client.GetAsync("/api/users");
        Assert.Equal(HttpStatusCode.OK, getAllResponse.StatusCode);

        var allUsers = await getAllResponse.Content.ReadFromJsonAsync<List<User>>(JsonOptions);
        Assert.NotNull(allUsers);
        Assert.Single(allUsers);
        Assert.Equal(createdUser.UserId, allUsers[0].UserId);
    }
}
