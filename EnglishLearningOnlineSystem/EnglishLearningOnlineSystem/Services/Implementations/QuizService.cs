using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Repositories.Interfaces;
using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.ViewModels.ContentManager.Quizzes;

namespace EnglishLearningOnlineSystem.Services.Implementations;

public class QuizService : IQuizService
{
    private readonly IQuizRepository _repo;

    public QuizService(IQuizRepository repo)
    {
        _repo = repo;
    }

    public async Task<(List<QuizListItemViewModel> Items, int TotalCount)> GetQuizzesAsync(string? keyword, int? lessonId, int page, int pageSize)
    {
        var (quizzes, totalCount) = await _repo.GetQuizzesPaginatedAsync(keyword, lessonId, page, pageSize);
        var items = quizzes.Select(q => new QuizListItemViewModel
        {
            QuizId = q.QuizId,
            Question = q.Question,
            QuizType = q.QuizType,
            LessonTitle = q.Lesson?.Title ?? "",
            CourseName = q.Lesson?.Course?.CourseName ?? ""
        }).ToList();

        return (items, totalCount);
    }

    public async Task<(QuizEditViewModel? Model, string? ErrorMessage)> GetQuizForEditAsync(int id)
    {
        var q = await _repo.GetQuizByIdAsync(id);
        if (q == null) return (null, "Không tìm thấy câu hỏi.");

        var model = new QuizEditViewModel
        {
            QuizId = q.QuizId,
            Question = q.Question,
            QuizType = q.QuizType,
            Options = q.Options,
            CorrectAnswer = q.CorrectAnswer,
            LessonId = q.LessonId
        };
        return (model, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> CreateQuizAsync(QuizCreateViewModel model)
    {
        var quiz = new Quiz
        {
            Question = model.Question,
            QuizType = model.QuizType,
            Options = model.Options,
            CorrectAnswer = model.CorrectAnswer,
            LessonId = model.LessonId
        };

        await _repo.AddQuizAsync(quiz);
        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> UpdateQuizAsync(QuizEditViewModel model)
    {
        var quiz = await _repo.GetQuizByIdAsync(model.QuizId);
        if (quiz == null) return (false, "Không tìm thấy câu hỏi.");

        quiz.Question = model.Question;
        quiz.QuizType = model.QuizType;
        quiz.Options = model.Options;
        quiz.CorrectAnswer = model.CorrectAnswer;
        quiz.LessonId = model.LessonId;

        await _repo.UpdateQuizAsync(quiz);
        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> DeleteQuizAsync(int id)
    {
        var quiz = await _repo.GetQuizByIdAsync(id);
        if (quiz == null) return (false, "Không tìm thấy câu hỏi.");

        await _repo.DeleteQuizAsync(quiz);
        return (true, null);
    }
}
