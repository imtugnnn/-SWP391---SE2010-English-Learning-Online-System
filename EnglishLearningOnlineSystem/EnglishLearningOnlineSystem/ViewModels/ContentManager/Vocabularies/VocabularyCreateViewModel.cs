using System.ComponentModel.DataAnnotations;

namespace EnglishLearningOnlineSystem.ViewModels.ContentManager.Vocabularies
{
    public class VocabularyCreateViewModel
    {
        [Required(ErrorMessage = "Từ vựng là bắt buộc")]
        [Display(Name = "Từ vựng")]
        public string Word { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nghĩa là bắt buộc")]
        [Display(Name = "Nghĩa")]
        public string Meaning { get; set; } = string.Empty;

        [Display(Name = "URL Hình ảnh")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Câu ví dụ")]
        public string? ExampleSentence { get; set; }

        [Display(Name = "URL Âm thanh")]
        public string? AudioUrl { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn bài học")]
        [Display(Name = "Bài học")]
        public int LessonId { get; set; }
    }
}
