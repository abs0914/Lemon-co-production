using LemonCo.Core.Models;

namespace LemonCo.Core.Interfaces;

/// <summary>
/// Service for managing items and BOMs
/// </summary>
public interface IItemService
{
    /// <summary>
    /// Search for items
    /// </summary>
    Task<List<Item>> SearchItemsAsync(string? search = null);

    /// <summary>
    /// Get item by code
    /// </summary>
    Task<Item?> GetItemAsync(string itemCode);

    /// <summary>
    /// Create a new item
    /// </summary>
    Task<Item> CreateItemAsync(Item item);

    /// <summary>
    /// Update an existing item
    /// </summary>
    Task<Item> UpdateItemAsync(Item item);

    /// <summary>
    /// Get BOM for an item
    /// </summary>
    Task<List<BomLine>> GetBomAsync(string itemCode);

    /// <summary>
    /// Create or update BOM for an item
    /// </summary>
    Task<bool> SaveBomAsync(string itemCode, List<BomLine> bomLines);

    /// <summary>
    /// Import BOM from CSV data
    /// </summary>
    Task<bool> ImportBomFromCsvAsync(string itemCode, string csvContent);
}

