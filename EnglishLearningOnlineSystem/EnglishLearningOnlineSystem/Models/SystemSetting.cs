using System;
using System.ComponentModel.DataAnnotations;

namespace EnglishLearningOnlineSystem.Models;

public class SystemSetting
{
    [Key]
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
