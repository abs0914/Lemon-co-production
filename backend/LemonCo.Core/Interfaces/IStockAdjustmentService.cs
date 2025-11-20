using LemonCo.Core.Models;

namespace LemonCo.Core.Interfaces;

/// <summary>
/// Service for creating stock adjustments in AutoCount.
/// </summary>
public interface IStockAdjustmentService
{
    /// <summary>
    /// Create a new stock adjustment document.
    /// </summary>
    Task<StockAdjustmentResult> CreateStockAdjustmentAsync(StockAdjustmentInput input);
}

