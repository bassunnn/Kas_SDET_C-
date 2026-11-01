namespace UserManagement.API.DTOs;

/// <summary>
/// DTO для входящих запросов создания и обновления пользователя.
/// Содержит только CountryIsoCode и Locale.
/// </summary>
public class UserDto
{
    /// <summary>
    /// Двухбуквенный код страны (регистронезависимый при вводе, строго 2 буквы латиницы).
    /// </summary>
    public string CountryIsoCode { get; set; } = string.Empty;

    /// <summary>
    /// Локаль пользователя в формате xx-XX.
    /// </summary>
    public string Locale { get; set; } = string.Empty;
}
