using LemonCo.Core.Models;

namespace LemonCo.Core.Interfaces;

/// <summary>
/// Service for managing sales orders (integration API)
/// </summary>
public interface ISalesOrderService
{
    /// <summary>
    /// Create a sales order from external platform
    /// </summary>
    Task<SalesOrderResult> CreateSalesOrderAsync(SalesOrderInput input);

    /// <summary>
    /// Validate customer exists in AutoCount
    /// </summary>
    Task<bool> ValidateCustomerAsync(string customerCode);

    /// <summary>
    /// Validate items exist in AutoCount
    /// </summary>
    Task<List<string>> ValidateItemsAsync(List<string> itemCodes);
}

