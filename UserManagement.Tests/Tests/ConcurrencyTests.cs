using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using UserManagement.API.DTOs;
using UserManagement.API.Models;
using UserManagement.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace UserManagement.Tests.Tests;

/// <summary>
/// Тесты многопоточности и потокобезопасности.
/// Проверяют корректность работы in-memory репозитория в условиях высоких параллельных нагрузок.
/// </summary>
public class ConcurrencyTests : TestBase
{
    public ConcurrencyTests(CustomWebApplicationFactory factory, ITestOutputHelper output)
        : base(factory, output)
    {
    }

    [Fact]
    public async Task ConcurrentUserCreation_ShouldPersistAllUsersWithoutLossOrRaceConditions()
    {
        const int concurrentRequests = 30;
        var createdUserIds = new ConcurrentBag<Guid>();

        // Запускаем параллельные запросы создания пользователей
        var tasks = Enumerable.Range(0, concurrentRequests).Select(async i =>
        {
            var dto = new UserDto
            {
                CountryIsoCode = "US",
                Locale = "en-US"
            };

            var response = await Client.PostAsJsonAsync("/api/users", dto);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var user = await response.Content.ReadFromJsonAsync<User>(JsonOptions);
            Assert.NotNull(user);
            createdUserIds.Add(user.UserId);
        });

        await Task.WhenAll(tasks);

        // Все созданные ID должны быть уникальными
        Assert.Equal(concurrentRequests, createdUserIds.Count);
        Assert.Equal(concurrentRequests, createdUserIds.Distinct().Count());

        var getAllResponse = await Client.GetAsync("/api/users");
        Assert.Equal(HttpStatusCode.OK, getAllResponse.StatusCode);

        var allUsers = await getAllResponse.Content.ReadFromJsonAsync<List<User>>(JsonOptions);
        Assert.NotNull(allUsers);
        Assert.Equal(concurrentRequests, allUsers.Count);
    }

    [Fact]
    public async Task ConcurrentReadsAndUpdates_OnSameUser_ShouldRemainConsistentAndSafe()
    {
        var initialDto = new UserDto
        {
            CountryIsoCode = "DE",
            Locale = "de-DE"
        };
        var createResponse = await Client.PostAsJsonAsync("/api/users", initialDto);
        var user = await createResponse.Content.ReadFromJsonAsync<User>(JsonOptions);
        Assert.NotNull(user);

        const int parallelWorkers = 20;
        var tasks = Enumerable.Range(0, parallelWorkers).Select(async i =>
        {
            if (i % 2 == 0)
            {
                var getRes = await Client.GetAsync($"/api/users/{user.UserId}");
                Assert.Equal(HttpStatusCode.OK, getRes.StatusCode);
            }
            else
            {
                var updateDto = new UserDto
                {
                    CountryIsoCode = "FR",
                    Locale = "fr-FR"
                };
                var putRes = await Client.PutAsJsonAsync($"/api/users/{user.UserId}", updateDto);
                Assert.Equal(HttpStatusCode.OK, putRes.StatusCode);
            }
        });

        await Task.WhenAll(tasks);

        var finalResponse = await Client.GetAsync($"/api/users/{user.UserId}");
        Assert.Equal(HttpStatusCode.OK, finalResponse.StatusCode);

        var finalUser = await finalResponse.Content.ReadFromJsonAsync<User>(JsonOptions);
        Assert.NotNull(finalUser);
        Assert.Equal(user.UserId, finalUser.UserId);
        Assert.NotNull(finalUser.DateModified);
    }
}
