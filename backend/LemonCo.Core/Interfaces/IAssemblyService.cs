using LemonCo.Core.Models;

namespace LemonCo.Core.Interfaces;

/// <summary>
/// Service for managing assembly orders
/// </summary>
public interface IAssemblyService
{
    /// <summary>
    /// Create a new assembly order
    /// </summary>
    Task<AssemblyOrder> CreateAssemblyOrderAsync(AssemblyOrderInput input);

    /// <summary>
    /// Get assembly order by document number
    /// </summary>
    Task<AssemblyOrder?> GetAssemblyOrderAsync(string docNo);

    /// <summary>
    /// Get all open assembly orders
    /// </summary>
    Task<List<AssemblyOrder>> GetOpenAssemblyOrdersAsync();

    /// <summary>
    /// Post an assembly order (consume materials, produce finished goods)
    /// </summary>
    Task<PostAssemblyResult> PostAssemblyAsync(PostAssemblyInput input);

    /// <summary>
    /// Cancel an assembly order
    /// </summary>
    Task<bool> CancelAssemblyOrderAsync(string docNo);
}

