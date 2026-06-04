namespace EnglishLearningOnlineSystem.Services.Models
{
    public class UserServiceResult<T>
    {
        public bool Succeeded { get; init; }
        public string? ErrorMessage { get; init; }
        public T? Data { get; init; }

        public static UserServiceResult<T> Ok(T? data = default)
            => new() { Succeeded = true, Data = data };

        public static UserServiceResult<T> Fail(string message)
            => new() { Succeeded = false, ErrorMessage = message };
    }
}
