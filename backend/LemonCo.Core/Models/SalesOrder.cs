namespace LemonCo.Core.Models;

/// <summary>
/// Input model for creating a sales order from external platform
/// </summary>
public class SalesOrderInput
{
    /// <summary>
    /// Customer code in AutoCount
    /// </summary>
    public string CustomerCode { get; set; } = string.Empty;

    /// <summary>
    /// Sales order lines
    /// </summary>
    public List<SalesOrderLine> Lines { get; set; } = new();

    /// <summary>
    /// Optional remarks
    /// </summary>
    public string? Remarks { get; set; }

    /// <summary>
    /// External reference number (from source system)
    /// </summary>
    public string? ExternalRef { get; set; }

    /// <summary>
    /// Delivery date
    /// </summary>
    public DateTime? DeliveryDate { get; set; }
}

/// <summary>
/// Sales order line item
/// </summary>
public class SalesOrderLine
{
    /// <summary>
    /// Item code
    /// </summary>
    public string ItemCode { get; set; } = string.Empty;

    /// <summary>
    /// Quantity ordered
    /// </summary>
    public decimal Qty { get; set; }

    /// <summary>
    /// Unit price (optional, will use AutoCount price if not provided)
    /// </summary>
    public decimal? UnitPrice { get; set; }

    /// <summary>
    /// Discount percentage
    /// </summary>
    public decimal? DiscountPercent { get; set; }

    /// <summary>
    /// Line remarks
    /// </summary>
    public string? Remarks { get; set; }
}

/// <summary>
/// Result of creating a sales order
/// </summary>
public class SalesOrderResult
{
    /// <summary>
    /// Sales order document number
    /// </summary>
    public string SoNo { get; set; } = string.Empty;

    /// <summary>
    /// Status message
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Whether creation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Validation errors (if any)
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Total amount
    /// </summary>
    public decimal? TotalAmount { get; set; }
}

