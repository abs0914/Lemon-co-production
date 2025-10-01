namespace LemonCo.Core.Models;

/// <summary>
/// Represents a stock item
/// </summary>
public class Item
{
    /// <summary>
    /// Item code (SKU)
    /// </summary>
    public string ItemCode { get; set; } = string.Empty;

    /// <summary>
    /// Item description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Base unit of measure
    /// </summary>
    public string BaseUom { get; set; } = string.Empty;

    /// <summary>
    /// Item type (e.g., Finished, Raw, Component)
    /// </summary>
    public string ItemType { get; set; } = string.Empty;

    /// <summary>
    /// Barcode
    /// </summary>
    public string? Barcode { get; set; }

    /// <summary>
    /// Whether this item has a BOM
    /// </summary>
    public bool HasBom { get; set; }

    /// <summary>
    /// Current stock balance
    /// </summary>
    public decimal StockBalance { get; set; }

    /// <summary>
    /// Standard cost
    /// </summary>
    public decimal StandardCost { get; set; }
}

