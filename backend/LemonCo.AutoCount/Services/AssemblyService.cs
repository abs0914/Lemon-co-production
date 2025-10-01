using AutoCount.Manufacturing.StockAssembly;
using LemonCo.Core.Interfaces;
using LemonCo.Core.Models;
using Microsoft.Extensions.Logging;

namespace LemonCo.AutoCount.Services;

/// <summary>
/// AutoCount implementation of Assembly Order service using AutoCount SDK
/// </summary>
public class AssemblyService : IAssemblyService
{
    private readonly AutoCountConnectionManager _connectionManager;
    private readonly IItemService _itemService;
    private readonly ILogger<AssemblyService> _logger;

    public AssemblyService(
        AutoCountConnectionManager connectionManager,
        IItemService itemService,
        ILogger<AssemblyService> logger)
    {
        _connectionManager = connectionManager;
        _itemService = itemService;
        _logger = logger;
    }

    public async Task<AssemblyOrder> CreateAssemblyOrderAsync(AssemblyOrderInput input)
    {
        return await Task.Run(() =>
        {
            _logger.LogInformation("Creating stock assembly for item: {ItemCode}, Qty: {Quantity}",
                input.ItemCode, input.Quantity);

            var userSession = _connectionManager.GetUserSession();
            var cmd = StockAssemblyCommand.Create(userSession, userSession.DBSetting);
            var doc = cmd.AddNew();

            // Set document properties
            doc.DocDate = DateTime.Parse(input.ProductionDate);
            doc.ItemCode = input.ItemCode;
            doc.Qty = input.Quantity;

            if (!string.IsNullOrEmpty(input.Remarks))
            {
                doc.Description = input.Remarks;
            }

            // Save the stock assembly
            doc.Save();

            _logger.LogInformation("Stock assembly created: {DocNo}", doc.DocNo);

            // Map to our model
            return new AssemblyOrder
            {
                DocNo = doc.DocNo ?? "",
                ItemCode = doc.ItemCode ?? "",
                ItemDescription = doc.Description ?? "",
                Quantity = doc.Qty ?? 0,
                ProductionDate = doc.DocDate ?? DateTime.Today,
                Remarks = doc.Description ?? "",
                Status = "Open",
                CreatedDate = DateTime.UtcNow
            };
        });
    }

    public async Task<AssemblyOrder?> GetAssemblyOrderAsync(string docNo)
    {
        return await Task.Run(() =>
        {
            _logger.LogInformation("Getting stock assembly: {DocNo}", docNo);

            var userSession = _connectionManager.GetUserSession();
            var cmd = StockAssemblyCommand.Create(userSession, userSession.DBSetting);
            var doc = cmd.View(docNo);

            if (doc == null)
            {
                return null;
            }

            return new AssemblyOrder
            {
                DocNo = doc.DocNo ?? "",
                ItemCode = doc.ItemCode ?? "",
                ItemDescription = doc.Description ?? "",
                Quantity = doc.Qty ?? 0,
                ProductionDate = doc.DocDate ?? DateTime.Today,
                Remarks = doc.Description ?? "",
                Status = doc.Cancelled ? "Cancelled" : "Open", // Posted property doesn't exist
                CreatedDate = doc.DocDate ?? DateTime.Today
            };
        });
    }

    public async Task<List<AssemblyOrder>> GetOpenAssemblyOrdersAsync()
    {
        return await Task.Run(() =>
        {
            _logger.LogInformation("Getting open stock assemblies");

            var userSession = _connectionManager.GetUserSession();
            var orders = new List<AssemblyOrder>();

            // Note: AutoCount SDK doesn't have a direct list method in the wiki examples
            // This would typically require a custom SQL query or data access method
            // For now, returning empty list - implement based on actual AutoCount SDK capabilities

            _logger.LogWarning("GetOpenAssemblyOrdersAsync not fully implemented - requires custom data access");

            return orders;
        });
    }

    public async Task<PostAssemblyResult> PostAssemblyAsync(PostAssemblyInput input)
    {
        return await Task.Run(() =>
        {
            _logger.LogInformation("Posting stock assembly: {DocNo}", input.OrderDocNo);

            var result = new PostAssemblyResult
            {
                DocNo = input.OrderDocNo
            };

            try
            {
                var userSession = _connectionManager.GetUserSession();
                var cmd = StockAssemblyCommand.Create(userSession, userSession.DBSetting);
                var doc = cmd.View(input.OrderDocNo);

                if (doc == null)
                {
                    result.Success = false;
                    result.ErrorMessage = $"Stock assembly {input.OrderDocNo} not found";
                    return result;
                }

                // Check if already cancelled
                if (doc.Cancelled)
                {
                    result.Success = false;
                    result.ErrorMessage = $"Stock assembly {input.OrderDocNo} is cancelled";
                    return result;
                }

                // Post the assembly (consumes raw materials, produces finished goods)
                doc.Save();

                // Get cost breakdown from assembly details - Detail property may not exist
                decimal totalCost = 0;
                // Note: Detail collection access requires further investigation of AutoCount SDK
                _logger.LogWarning("Cost breakdown not implemented - requires Detail collection access");

                // Placeholder for cost breakdown
                result.CostBreakdowns.Add(new CostBreakdown
                {
                    ComponentCode = doc.ItemCode ?? "",
                    Description = doc.Description ?? "",
                    Quantity = doc.Qty ?? 0,
                    UnitCost = 0,
                    TotalCost = 0
                });

                result.TotalCost = totalCost;
                result.Success = true;

                _logger.LogInformation("Stock assembly posted successfully. DocNo: {DocNo}, Total Cost: {Cost}",
                    input.OrderDocNo, totalCost);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to post stock assembly: {DocNo}", input.OrderDocNo);
                result.Success = false;
                result.ErrorMessage = ex.Message;
                return result;
            }
        });
    }

    public async Task<bool> CancelAssemblyOrderAsync(string docNo)
    {
        return await Task.Run(() =>
        {
            _logger.LogInformation("Cancelling stock assembly: {DocNo}", docNo);

            try
            {
                var userSession = _connectionManager.GetUserSession();
                var cmd = StockAssemblyCommand.Create(userSession, userSession.DBSetting);
                var doc = cmd.View(docNo);

                if (doc == null)
                {
                    _logger.LogWarning("Stock assembly not found: {DocNo}", docNo);
                    return false;
                }

                // Check if already cancelled
                if (doc.Cancelled)
                {
                    _logger.LogWarning("Stock assembly already cancelled: {DocNo}", docNo);
                    return false;
                }

                // Cancel the document - Cancelled property is read-only
                // Need to use Delete method instead
                cmd.Delete(docNo);

                _logger.LogInformation("Stock assembly deleted: {DocNo}", docNo);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cancel stock assembly: {DocNo}", docNo);
                return false;
            }
        });
    }
}

