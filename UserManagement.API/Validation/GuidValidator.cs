namespace UserManagement.API.Validation;

public static class GuidValidator
{
    /// <summary>
    /// Проверяет, является ли переданная строка валидным непустым GUID.
    /// </summary>
    public static bool TryValidate(string? rawId, out Guid parsedGuid, out string? errorMessage)
    {
        if (string.IsNullOrWhiteSpace(rawId))
        {
            parsedGuid = Guid.Empty;
            errorMessage = "UserId route parameter is missing or empty.";
            return false;
        }

        if (!Guid.TryParse(rawId, out parsedGuid))
        {
            errorMessage = $"UserId '{rawId}' is not a valid GUID format.";
            return false;
        }

        if (parsedGuid == Guid.Empty)
        {
            errorMessage = "UserId cannot be an empty GUID (Guid.Empty: 00000000-0000-0000-0000-000000000000 is not allowed).";
            return false;
        }

        errorMessage = null;
        return true;
    }
}
