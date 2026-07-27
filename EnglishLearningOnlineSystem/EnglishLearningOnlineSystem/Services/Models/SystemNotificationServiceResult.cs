namespace EnglishLearningOnlineSystem.Services.Models;

public class SystemNotificationServiceResult<T>
{
    public bool Succeeded { get; init; }
    public string? ErrorMessage { get; init; }
    public T? Data { get; init; }

    public static SystemNotificationServiceResult<T> Ok(T? data = default)
        => new() { Succeeded = true, Data = data };

    public static SystemNotificationServiceResult<T> Fail(string message)
        => new() { Succeeded = false, ErrorMessage = message };
}
