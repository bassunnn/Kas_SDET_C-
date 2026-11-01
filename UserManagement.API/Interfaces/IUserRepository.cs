using UserManagement.API.Models;

namespace UserManagement.API.Interfaces;

/// <summary>
/// Интерфейс репозитория для управления пользователями.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Добавляет нового пользователя, возвращает созданный объект.
    /// </summary>
    User Add(User user);

    /// <summary>
    /// Обновляет данные существующего пользователя, возвращает true/false.
    /// </summary>
    bool Update(User user);

    /// <summary>
    /// Возвращает пользователя по ID или null, если не найден.
    /// </summary>
    User? GetById(Guid userId);

    /// <summary>
    /// Возвращает всех пользователей.
    /// </summary>
    IEnumerable<User> GetAll();

    /// <summary>
    /// Очищает все записи в хранилище (используется для изоляции тестов).
    /// </summary>
    void Clear();
}
