namespace LemonCo.Core.Models;

/// <summary>
/// Input model for printing labels
/// </summary>
public class LabelPrintInput
{
    /// <summary>
    /// Item code to print label for
    /// </summary>
    public string ItemCode { get; set; } = string.Empty;

    /// <summary>
    /// Batch number (optional)
    /// </summary>
    public string? BatchNo { get; set; }

    /// <summary>
    /// Manufacturing date (ISO 8601 format)
    /// </summary>
    public string? MfgDate { get; set; }

    /// <summary>
    /// Expiry date (ISO 8601 format)
    /// </summary>
    public string? ExpDate { get; set; }

    /// <summary>
    /// Number of copies to print
    /// </summary>
    public int Copies { get; set; } = 1;

    /// <summary>
    /// Output format (ZPL or PDF)
    /// </summary>
    public string Format { get; set; } = "ZPL";
}

/// <summary>
/// Result of label printing
/// </summary>
public class LabelPrintResult
{
    /// <summary>
    /// Whether printing was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Generated label content (ZPL code or base64 PDF)
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// Content type (application/pdf or text/plain)
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Error message if failed
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Barcode configuration
/// </summary>
public class BarcodeConfig
{
    /// <summary>
    /// Barcode type (Code128, QR)
    /// </summary>
    public string Type { get; set; } = "Code128";

    /// <summary>
    /// Quantity separator character for scanner input
    /// </summary>
    public string QtySeparator { get; set; } = "*";

    /// <summary>
    /// Whether to include quantity in barcode
    /// </summary>
    public bool IncludeQty { get; set; } = false;
}

