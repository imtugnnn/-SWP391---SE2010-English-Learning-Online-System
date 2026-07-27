namespace EnglishLearningOnlineSystem.ViewModels.Admin.AcademicYears;

public class AcademicYearValidationError
{
    public AcademicYearValidationError(string key, string message)
    {
        Key = key;
        Message = message;
    }

    public string Key { get; }
    public string Message { get; }
}

public class AcademicYearActionResult
{
    public bool Success { get; set; }

    public bool NotFound { get; set; }

    public string? SuccessMessage { get; set; }

    public string? ErrorMessage { get; set; }

    public List<AcademicYearValidationError> Errors { get; } = new();
}

public class AcademicYearCreateResult : AcademicYearActionResult
{
    public int? AcademicYearId { get; set; }
}

public class AcademicYearEditResult : AcademicYearActionResult
{
    public AcademicYearEditViewModel? ViewModel { get; set; }
}
