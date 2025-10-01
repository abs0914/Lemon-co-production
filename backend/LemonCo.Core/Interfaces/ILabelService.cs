using LemonCo.Core.Models;

namespace LemonCo.Core.Interfaces;

/// <summary>
/// Service for generating and printing labels
/// </summary>
public interface ILabelService
{
    /// <summary>
    /// Generate label for printing
    /// </summary>
    Task<LabelPrintResult> GenerateLabelAsync(LabelPrintInput input);

    /// <summary>
    /// Get barcode configuration
    /// </summary>
    Task<BarcodeConfig> GetBarcodeConfigAsync();

    /// <summary>
    /// Update barcode configuration
    /// </summary>
    Task<bool> UpdateBarcodeConfigAsync(BarcodeConfig config);

    /// <summary>
    /// Parse barcode scanner input with quantity separator
    /// </summary>
    (string itemCode, decimal quantity) ParseBarcodeInput(string input);
}

