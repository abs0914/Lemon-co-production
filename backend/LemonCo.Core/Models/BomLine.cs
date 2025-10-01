namespace LemonCo.Core.Models;

/// <summary>
/// Represents a single line in a Bill of Materials
/// </summary>
public class BomLine
{
    /// <summary>
    /// Component item code
    /// </summary>
    public string ComponentCode { get; set; } = string.Empty;

    /// <summary>
    /// Quantity per unit of finished item
    /// </summary>
    public decimal QtyPer { get; set; }

    /// <summary>
    /// Unit of measure
    /// </summary>
    public string Uom { get; set; } = string.Empty;

    /// <summary>
    /// Component description (optional, for display)
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Sequence number for ordering
    /// </summary>
    public int Sequence { get; set; }
}

