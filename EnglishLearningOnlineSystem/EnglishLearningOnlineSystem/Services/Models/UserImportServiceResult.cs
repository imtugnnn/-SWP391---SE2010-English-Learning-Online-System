namespace EnglishLearningOnlineSystem.Services.Models;

public class UserImportServiceResult
{
    public bool Succeeded { get; init; }
    public int ImportedCount { get; init; }
    public string Message { get; init; } = string.Empty;
    public List<string> Errors { get; init; } = new();

    public static UserImportServiceResult Ok(int importedCount, string message)
        => new()
        {
            Succeeded = true,
            ImportedCount = importedCount,
            Message = message
        };

    public static UserImportServiceResult Fail(string message, IEnumerable<string>? errors = null)
        => new()
        {
            Succeeded = false,
            Message = message,
            Errors = errors?.ToList() ?? new List<string>()
        };
}
