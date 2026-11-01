namespace UserManagement.API.Models;

/// <summary>
/// Представляет сущность пользователя в системе.
/// </summary>
public class User
{
    /// <summary>
    /// Уникальный идентификатор пользователя.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Двухбуквенный код страны в верхнем регистре.
    /// </summary>
    public string CountryIsoCode { get; set; } = string.Empty;

    /// <summary>
    /// Дата и время создания пользователя в UTC.
    /// </summary>
    public DateTime DateCreated { get; set; }

    /// <summary>
    /// Дата и время последнего изменения пользователя в UTC.
    /// </summary>
    public DateTime? DateModified { get; set; }

    /// <summary>
    /// Локаль пользователя в формате xx-XX.
    /// </summary>
    public string Locale { get; set; } = string.Empty;

    /// <summary>
    /// Создает защитную копию объекта
    /// для предотвращения состояния гонки и утечек изменяемых ссылок.
    /// </summary>
    public User Clone() => new()
    {
        UserId = UserId,
        CountryIsoCode = CountryIsoCode,
        DateCreated = DateCreated,
        DateModified = DateModified,
        Locale = Locale
    };
}
