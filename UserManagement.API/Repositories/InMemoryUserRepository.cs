using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using UserManagement.API.Interfaces;
using UserManagement.API.Models;

namespace UserManagement.API.Repositories;

/// <summary>
/// Потокобезопасная in-memory реализация репозитория пользователей.
/// Использует ConcurrentDictionary с возвратом защитных копий
/// для предотвращения гонки условий и утечек изменяемого состояния.
/// </summary>
public class InMemoryUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<Guid, User> _users = new();
    private readonly ILogger<InMemoryUserRepository>? _logger;

    public InMemoryUserRepository(ILogger<InMemoryUserRepository>? logger = null)
    {
        _logger = logger;
    }

    public User Add(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var copy = user.Clone();
        _users[copy.UserId] = copy;
        _logger?.LogDebug("User {UserId} added to repository. Total users: {Count}", copy.UserId, _users.Count);
        return copy.Clone();
    }

    public bool Update(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        // Используем цикл с TryUpdate (Compare-And-Swap) для потокобезопасного обновления
        while (_users.TryGetValue(user.UserId, out var existing))
        {
            var updatedCopy = user.Clone();
            if (_users.TryUpdate(user.UserId, updatedCopy, existing))
            {
                _logger?.LogDebug("User {UserId} successfully updated in repository", user.UserId);
                return true;
            }
        }

        _logger?.LogDebug("Attempted to update non-existent user {UserId}", user.UserId);
        return false;
    }

    public User? GetById(Guid userId)
    {
        if (_users.TryGetValue(userId, out var user))
        {
            return user.Clone();
        }

        return null;
    }

    public IEnumerable<User> GetAll()
    {
        // Возвращаем защитные копии всех пользователей, отсортированные по дате создания
        return _users.Values
            .Select(u => u.Clone())
            .OrderBy(u => u.DateCreated)
            .ToList();
    }

    public void Clear()
    {
        _users.Clear();
        _logger?.LogDebug("Repository cleared");
    }
}
