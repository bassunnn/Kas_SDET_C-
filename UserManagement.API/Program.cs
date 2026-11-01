using FluentValidation;
using UserManagement.API.Interfaces;
using UserManagement.API.Middlewares;
using UserManagement.API.Repositories;
using UserManagement.API.Validation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();

builder.Services.AddValidatorsFromAssemblyContaining<UserDtoValidator>();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Подключение Custom Middleware глобальной обработки исключений из доп задания 2
app.UseCustomExceptionHandling();

app.UseRouting();

app.MapControllers();

app.Run();

// Делаем Program доступным для WebApplicationFactory в проекте тестов
public partial class Program { }
