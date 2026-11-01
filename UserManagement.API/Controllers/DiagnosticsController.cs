using Microsoft.AspNetCore.Mvc;

namespace UserManagement.API.Controllers;

/// <summary>
/// Контроллер для диагностики и тестирования системных компонентов (в частности, Custom Exception Middleware).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(IgnoreApi = true)]
public class DiagnosticsController : ControllerBase
{
    /// <summary>
    /// Эндпоинт, намеренно генерирующий необработанное исключение
    /// для проверки перехвата Custom Exception Handling Middleware.
    /// </summary>
    [HttpGet("throw-unhandled")]
    public IActionResult ThrowUnhandled()
    {
        throw new InvalidOperationException("Simulated unexpected exception for middleware verification.");
    }
}
