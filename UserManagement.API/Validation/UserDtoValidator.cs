using System.Text.RegularExpressions;
using FluentValidation;
using UserManagement.API.DTOs;

namespace UserManagement.API.Validation;

/// <summary>
/// Валидатор для UserDto согласно требованиям Этапа 3.
/// </summary>
public partial class UserDtoValidator : AbstractValidator<UserDto>
{
    private static readonly Regex CountryIsoCodeRegex = CountryIsoRegexGenerator();
    private static readonly Regex LocaleRegex = LocaleRegexGenerator();

    public UserDtoValidator()
    {
        RuleFor(x => x.CountryIsoCode)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage("CountryIsoCode is required and cannot be null.")
            .NotEmpty()
            .WithMessage("CountryIsoCode is required and cannot be empty.")
            .Must(code => CountryIsoCodeRegex.IsMatch(code?.Trim() ?? string.Empty))
            .WithMessage("CountryIsoCode must consist of strictly 2 Latin letters (e.g. 'RU', 'US').");

        RuleFor(x => x.Locale)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage("Locale is required and cannot be null.")
            .NotEmpty()
            .WithMessage("Locale is required and cannot be empty.")
            .Must(locale => LocaleRegex.IsMatch(locale?.Trim() ?? string.Empty))
            .WithMessage("Locale must strictly follow the format 'xx-XX' (two lowercase Latin letters, hyphen, two uppercase Latin letters, e.g. 'ru-RU', 'en-US').");
    }

    [GeneratedRegex("^[a-zA-Z]{2}$", RegexOptions.Compiled)]
    private static partial Regex CountryIsoRegexGenerator();

    [GeneratedRegex("^[a-z]{2}-[A-Z]{2}$", RegexOptions.Compiled)]
    private static partial Regex LocaleRegexGenerator();
}
