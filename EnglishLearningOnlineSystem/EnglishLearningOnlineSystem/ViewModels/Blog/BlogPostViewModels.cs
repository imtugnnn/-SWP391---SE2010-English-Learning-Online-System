using System.ComponentModel.DataAnnotations;

namespace EnglishLearningOnlineSystem.ViewModels.Blog
{
    public class BlogPostListItemViewModel
    {
        public int BlogPostId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsPublished { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
    }

    public class BlogPostEditViewModel
    {
        public int BlogPostId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề.")]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Summary { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung.")]
        public string Content { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn danh mục.")]
        [StringLength(50)]
        public string Category { get; set; } = "Grammar";

        public bool IsPublished { get; set; }
    }

    public class BlogReadListItemViewModel
    {
        public int BlogPostId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public string Category { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public DateTime? PublishedAt { get; set; }
    }

    public class BlogReadDetailViewModel
    {
        public int BlogPostId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public DateTime? PublishedAt { get; set; }
    }
}
