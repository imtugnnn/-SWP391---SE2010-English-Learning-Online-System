//Created by TungDPL
//Last update: 7/21/2026
namespace EnglishLearningOnlineSystem.Services.Models;

public class AuthServiceResult
{
    private AuthServiceResult(bool succeeded, IReadOnlyDictionary<string, string> errors)
    {
        Succeeded = succeeded;
        Errors = errors;
    }

    public bool Succeeded { get; }

    public IReadOnlyDictionary<string, string> Errors { get; }

    public static AuthServiceResult Success()
    {
        return new AuthServiceResult(true, new Dictionary<string, string>());
    }

    public static AuthServiceResult Failure(params (string Field, string Message)[] errors)
    {
        return new AuthServiceResult(
            false,
            errors.ToDictionary(error => error.Field, error => error.Message));
    }
}
