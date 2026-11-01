using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserManagement.API.DTOs;
using UserManagement.API.Interfaces;
using UserManagement.API.Models;
using UserManagement.API.Validation;

namespace UserManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _repository;
    private readonly IValidator<UserDto> _validator;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        IUserRepository repository,
        IValidator<UserDto> validator,
        ILogger<UsersController> logger)
    {
        _repository = repository;
        _validator = validator;
        _logger = logger;
    }

    /// <summary>
    /// Создание нового пользователя.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(User), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    public IActionResult CreateUser([FromBody] UserDto? dto)
    {
        if (dto is null)
        {
            _logger.LogWarning("CreateUser failed: Request body is empty or null");
            return BadRequest(new ErrorResponseDto
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad Request",
                Detail = "Request body cannot be null or empty."
            });
        }

        var validationResult = _validator.Validate(dto);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.ToDictionary();
            _logger.LogWarning("CreateUser validation failed with {Count} errors", validationResult.Errors.Count);
            return BadRequest(new ErrorResponseDto
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Failed",
                Detail = "One or more validation errors occurred.",
                Errors = errors
            });
        }

        var newUser = new User
        {
            UserId = Guid.NewGuid(),
            CountryIsoCode = dto.CountryIsoCode.Trim().ToUpperInvariant(),
            DateCreated = DateTime.UtcNow,
            DateModified = null,
            Locale = dto.Locale.Trim()
        };

        var created = _repository.Add(newUser);
        _logger.LogInformation("Created new user with ID {UserId}", created.UserId);

        return CreatedAtAction(nameof(GetById), new { userId = created.UserId }, created);
    }

    /// <summary>
    /// Получение пользователя по уникальному идентификатору.
    /// </summary>
    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    public IActionResult GetById([FromRoute] string userId)
    {
        if (!GuidValidator.TryValidate(userId, out var guid, out var errorMessage))
        {
            _logger.LogWarning("GetById rejected invalid userId '{UserId}': {Error}", userId, errorMessage);
            return BadRequest(new ErrorResponseDto
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid Identifier",
                Detail = errorMessage!
            });
        }

        var user = _repository.GetById(guid);
        if (user is null)
        {
            _logger.LogInformation("GetById: User with ID {UserId} was not found", guid);
            return NotFound(new ErrorResponseDto
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Not Found",
                Detail = $"User with ID '{guid}' was not found."
            });
        }

        return Ok(user);
    }

    /// <summary>
    /// Обновление существующего пользователя.
    /// Изменяет CountryIsoCode, Locale и обновляет DateModified.
    /// </summary>
    [HttpPut("{userId}")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    public IActionResult UpdateUser([FromRoute] string userId, [FromBody] UserDto? dto)
    {
        if (!GuidValidator.TryValidate(userId, out var guid, out var idError))
        {
            _logger.LogWarning("UpdateUser rejected invalid userId '{UserId}': {Error}", userId, idError);
            return BadRequest(new ErrorResponseDto
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid Identifier",
                Detail = idError!
            });
        }

        if (dto is null)
        {
            _logger.LogWarning("UpdateUser failed for {UserId}: Request body is null", guid);
            return BadRequest(new ErrorResponseDto
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad Request",
                Detail = "Request body cannot be null or empty."
            });
        }

        var validationResult = _validator.Validate(dto);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.ToDictionary();
            _logger.LogWarning("UpdateUser validation failed for {UserId} with {Count} errors", guid, validationResult.Errors.Count);
            return BadRequest(new ErrorResponseDto
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Failed",
                Detail = "One or more validation errors occurred.",
                Errors = errors
            });
        }

        var existingUser = _repository.GetById(guid);
        if (existingUser is null)
        {
            _logger.LogInformation("UpdateUser: User with ID {UserId} was not found", guid);
            return NotFound(new ErrorResponseDto
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Not Found",
                Detail = $"User with ID '{guid}' was not found."
            });
        }

        existingUser.CountryIsoCode = dto.CountryIsoCode.Trim().ToUpperInvariant();
        existingUser.Locale = dto.Locale.Trim();
        existingUser.DateModified = DateTime.UtcNow;

        _repository.Update(existingUser);
        _logger.LogInformation("Updated user with ID {UserId}", guid);

        return Ok(existingUser);
    }

    /// <summary>
    /// Получение списка пользователей с опциональной поддержкой пагинации.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<User>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    public IActionResult GetAll([FromQuery] int? page = null, [FromQuery] int? pageSize = null)
    {
        if (page.HasValue && page.Value < 1)
        {
            return BadRequest(new ErrorResponseDto
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid Pagination",
                Detail = "Page number must be greater than or equal to 1."
            });
        }

        if (pageSize.HasValue && pageSize.Value < 1)
        {
            return BadRequest(new ErrorResponseDto
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid Pagination",
                Detail = "Page size must be greater than or equal to 1."
            });
        }

        if (pageSize.HasValue && pageSize.Value > PaginationQueryDto.MaxPageSize)
        {
            return BadRequest(new ErrorResponseDto
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid Pagination",
                Detail = $"Page size cannot exceed {PaginationQueryDto.MaxPageSize}."
            });
        }

        var allUsers = _repository.GetAll().ToList();

        // Если параметры пагинации не переданы, возвращаем весь список
        if (!page.HasValue && !pageSize.HasValue)
        {
            Response.Headers.Append("X-Total-Count", allUsers.Count.ToString());
            return Ok(allUsers);
        }

        var currentPage = page ?? PaginationQueryDto.DefaultPage;
        var currentPageSize = pageSize ?? PaginationQueryDto.DefaultPageSize;

        var pagedUsers = allUsers
            .Skip((currentPage - 1) * currentPageSize)
            .Take(currentPageSize)
            .ToList();

        var totalPages = (int)Math.Ceiling(allUsers.Count / (double)currentPageSize);

        Response.Headers.Append("X-Total-Count", allUsers.Count.ToString());
        Response.Headers.Append("X-Page", currentPage.ToString());
        Response.Headers.Append("X-Page-Size", currentPageSize.ToString());
        Response.Headers.Append("X-Total-Pages", totalPages.ToString());

        return Ok(pagedUsers);
    }
}
