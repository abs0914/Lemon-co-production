using System.ComponentModel.DataAnnotations;

namespace LemonCo.Data.Entities;

/// <summary>
/// Application configuration key-value store
/// </summary>
public class AppConfig
{
    [Key]
    [MaxLength(100)]
    public string Key { get; set; } = string.Empty;

    [Required]
    public string Value { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

