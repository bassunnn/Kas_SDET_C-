using System.Net;
using System.Net.Http.Json;
using UserManagement.API.DTOs;
using UserManagement.API.Models;
using UserManagement.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace UserManagement.Tests.Tests;

/// <summary>
/// Тесты для дополнительного задания №1: Пагинация в GET /api/users.
/// Проверяет корректность разбиения на страницы, заголовки пагинации и граничные значения.
/// </summary>
public class PaginationTests : TestBase
{
    public PaginationTests(CustomWebApplicationFactory factory, ITestOutputHelper output)
        : base(factory, output)
    {
    }

    private async Task<List<User>> SeedUsersAsync(int count)
    {
        var users = new List<User>();
        for (int i = 1; i <= count; i++)
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
            users.Add(user);
        }
        return users;
    }

    [Fact]
    public async Task GetUsers_WithPagination_ShouldCorrectlySplitIntoPages()
    {
        var seeded = await SeedUsersAsync(5);

        var responsePage1 = await Client.GetAsync("/api/users?page=1&pageSize=2");
        Assert.Equal(HttpStatusCode.OK, responsePage1.StatusCode);

        var page1Users = await responsePage1.Content.ReadFromJsonAsync<List<User>>(JsonOptions);
        Assert.NotNull(page1Users);
        Assert.Equal(2, page1Users.Count);
        Assert.Equal(seeded[0].UserId, page1Users[0].UserId);
        Assert.Equal(seeded[1].UserId, page1Users[1].UserId);

        Assert.True(responsePage1.Headers.Contains("X-Total-Count"));
        Assert.Equal("5", responsePage1.Headers.GetValues("X-Total-Count").First());
        Assert.Equal("1", responsePage1.Headers.GetValues("X-Page").First());
        Assert.Equal("2", responsePage1.Headers.GetValues("X-Page-Size").First());
        Assert.Equal("3", responsePage1.Headers.GetValues("X-Total-Pages").First());

        var responsePage2 = await Client.GetAsync("/api/users?page=2&pageSize=2");
        Assert.Equal(HttpStatusCode.OK, responsePage2.StatusCode);

        var page2Users = await responsePage2.Content.ReadFromJsonAsync<List<User>>(JsonOptions);
        Assert.NotNull(page2Users);
        Assert.Equal(2, page2Users.Count);
        Assert.Equal(seeded[2].UserId, page2Users[0].UserId);
        Assert.Equal(seeded[3].UserId, page2Users[1].UserId);

        var responsePage3 = await Client.GetAsync("/api/users?page=3&pageSize=2");
        Assert.Equal(HttpStatusCode.OK, responsePage3.StatusCode);

        var page3Users = await responsePage3.Content.ReadFromJsonAsync<List<User>>(JsonOptions);
        Assert.NotNull(page3Users);
        Assert.Single(page3Users);
        Assert.Equal(seeded[4].UserId, page3Users[0].UserId);

        var responsePage4 = await Client.GetAsync("/api/users?page=4&pageSize=2");
        Assert.Equal(HttpStatusCode.OK, responsePage4.StatusCode);

        var page4Users = await responsePage4.Content.ReadFromJsonAsync<List<User>>(JsonOptions);
        Assert.NotNull(page4Users);
        Assert.Empty(page4Users);
    }

    [Theory]
    [InlineData("page=0&pageSize=10")]
    [InlineData("page=-1&pageSize=10")]
    [InlineData("page=1&pageSize=0")]
    [InlineData("page=1&pageSize=-5")]
    [InlineData("page=1&pageSize=101")] // Превышение максимального размера страницы
    [InlineData("page=abc&pageSize=10")] // Нечисловой параметр
    public async Task GetUsers_WithInvalidPaginationParams_ShouldReturnBadRequest(string query)
    {
        var response = await Client.GetAsync($"/api/users?{query}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponseDto>(JsonOptions);
        Assert.NotNull(error);
        Assert.Equal(400, error.Status);
    }
}
