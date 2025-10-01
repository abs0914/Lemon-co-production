namespace LemonCo.Core.Models;

/// <summary>
/// Represents an assembly/production order
/// </summary>
public class AssemblyOrder
{
    /// <summary>
    /// Document number (assigned by AutoCount)
    /// </summary>
    public string DocNo { get; set; } = string.Empty;

    /// <summary>
    /// Finished item code to produce
    /// </summary>
    public string ItemCode { get; set; } = string.Empty;

    /// <summary>
    /// Item description
    /// </summary>
    public string? ItemDescription { get; set; }

    /// <summary>
    /// Quantity to produce
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// Production date
    /// </summary>
    public DateTime ProductionDate { get; set; }

    /// <summary>
    /// Remarks/notes
    /// </summary>
    public string? Remarks { get; set; }

    /// <summary>
    /// Status (Open, Posted, Cancelled)
    /// </summary>
    public string Status { get; set; } = "Open";

    /// <summary>
    /// Created date
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Posted date (if posted)
    /// </summary>
    public DateTime? PostedDate { get; set; }

    /// <summary>
    /// Total cost (after posting)
    /// </summary>
    public decimal? TotalCost { get; set; }
}

/// <summary>
/// Input model for creating an assembly order
/// </summary>
public class AssemblyOrderInput
{
    /// <summary>
    /// Finished item code to produce
    /// </summary>
    public string ItemCode { get; set; } = string.Empty;

    /// <summary>
    /// Quantity to produce
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// Production date (ISO 8601 format)
    /// </summary>
    public string ProductionDate { get; set; } = string.Empty;

    /// <summary>
    /// Optional remarks
    /// </summary>
    public string? Remarks { get; set; }
}

/// <summary>
/// Input model for posting an assembly
/// </summary>
public class PostAssemblyInput
{
    /// <summary>
    /// Assembly order document number to post
    /// </summary>
    public string OrderDocNo { get; set; } = string.Empty;
}

/// <summary>
/// Result of posting an assembly
/// </summary>
public class PostAssemblyResult
{
    /// <summary>
    /// Posted document number
    /// </summary>
    public string DocNo { get; set; } = string.Empty;

    /// <summary>
    /// Whether posting was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Total cost of materials consumed
    /// </summary>
    public decimal TotalCost { get; set; }

    /// <summary>
    /// Cost breakdown by component
    /// </summary>
    public List<CostBreakdown> CostBreakdowns { get; set; } = new();
}

/// <summary>
/// Cost breakdown for a component
/// </summary>
public class CostBreakdown
{
    public string ComponentCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
}

