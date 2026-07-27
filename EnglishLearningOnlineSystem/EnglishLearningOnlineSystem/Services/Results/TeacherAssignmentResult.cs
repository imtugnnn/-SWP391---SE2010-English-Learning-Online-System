using EnglishLearningOnlineSystem.ViewModels;

namespace EnglishLearningOnlineSystem.Services.Results;

public sealed class TeacherAssignmentResult
{
    public bool Succeeded { get; init; }
    public string? ErrorMessage { get; init; }
    public AssignWeeklyLessonViewModel? FormModel { get; init; }

    public static TeacherAssignmentResult Success() => new() { Succeeded = true };

    public static TeacherAssignmentResult Failure(
        string errorMessage,
        AssignWeeklyLessonViewModel? formModel = null)
    {
        return new TeacherAssignmentResult
        {
            ErrorMessage = errorMessage,
            FormModel = formModel
        };
    }
}
