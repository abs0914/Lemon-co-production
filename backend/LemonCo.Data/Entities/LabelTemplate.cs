using System.ComponentModel.DataAnnotations;

namespace LemonCo.Data.Entities;

/// <summary>
/// Label template for barcode printing
/// </summary>
public class LabelTemplate
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Format { get; set; } = "ZPL"; // ZPL or PDF

    [Required]
    public string TemplateContent { get; set; } = string.Empty; // ZPL code or template

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsDefault { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}

