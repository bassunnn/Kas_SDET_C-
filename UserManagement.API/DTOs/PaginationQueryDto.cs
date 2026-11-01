namespace UserManagement.API.DTOs;

/// <summary>
/// Параметры пагинации для списочных запросов.
/// </summary>
public class PaginationQueryDto
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 10;
    public const int MaxPageSize = 100;

    /// <summary>
    /// Номер страницы начиная с 1.
    /// </summary>
    public int? Page { get; set; }

    /// <summary>
    /// Размер страницы (от 1 до 100).
    /// </summary>
    public int? PageSize { get; set; }
}
