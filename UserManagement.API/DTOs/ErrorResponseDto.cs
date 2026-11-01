namespace UserManagement.API.DTOs;

/// <summary>
/// Стандартизированная модель ответа об ошибке.
/// </summary>
public class ErrorResponseDto
{
    /// <summary>
    /// HTTP статус-код ошибки.
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// Краткий заголовок ошибки.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Подробное понятное описание ошибки.
    /// </summary>
    public string Detail { get; set; } = string.Empty;

    /// <summary>
    /// Идентификатор трассировки для сопоставления с серверными логами.
    /// </summary>
    public string? TraceId { get; set; }

    /// <summary>
    /// Словарь ошибок валидации по полям.
    /// </summary>
    public IDictionary<string, string[]>? Errors { get; set; }
}
