using LemonCo.Core.Models;

namespace LemonCo.Core.Interfaces;

/// <summary>
/// Service for accessing suppliers (AutoCount AP Creditors)
/// </summary>
public interface ISupplierService
{
    /// <summary>
    /// Get list of suppliers
    /// </summary>
    Task<List<Supplier>> GetSuppliersAsync(string? search = null);

    /// <summary>
    /// Get a single supplier by code
    /// </summary>
    Task<Supplier?> GetSupplierAsync(string code);
}

