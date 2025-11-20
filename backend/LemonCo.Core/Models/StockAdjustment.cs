namespace LemonCo.Core.Models;

/// <summary>
/// A single line in a stock adjustment document.
/// </summary>
public class StockAdjustmentLine
{
    /// <summary>
    /// Item code to adjust.
    /// </summary>
    public string ItemCode { get; set; } = string.Empty;

    /// <summary>
    /// Quantity to adjust. Positive = increase stock, negative = decrease stock.
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// Unit cost. Required when increasing stock (positive quantity).
    /// For negative quantity it is optional and for reference only.
    /// </summary>
    public decimal? UnitCost { get; set; }
}

/// <summary>
/// Input model for creating a stock adjustment.
/// </summary>
public class StockAdjustmentInput
{
    /// <summary>
    /// Document date in ISO 8601 format (e.g. 2025-01-31).
    /// </summary>
    public string DocDate { get; set; } = string.Empty;

    /// <summary>
    /// Description / remarks for the adjustment.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Reference document number.
    /// </summary>
    public string? RefDocNo { get; set; }

    /// <summary>
    /// Lines in the adjustment.
    /// </summary>
    public List<StockAdjustmentLine> Lines { get; set; } = new();
}

/// <summary>
/// Result of creating a stock adjustment.
/// </summary>
public class StockAdjustmentResult
{
    /// <summary>
    /// Document number assigned by AutoCount.
    /// </summary>
    public string DocNo { get; set; } = string.Empty;

    /// <summary>
    /// Whether the adjustment was created successfully.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if creation failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
}

