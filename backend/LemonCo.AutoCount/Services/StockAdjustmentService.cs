using AutoCount.Stock.StockAdjustment;
using LemonCo.Core.Interfaces;
using LemonCo.Core.Models;
using Microsoft.Extensions.Logging;

namespace LemonCo.AutoCount.Services;

/// <summary>
/// AutoCount implementation of stock adjustment service using AutoCount SDK.
/// </summary>
public class StockAdjustmentService : IStockAdjustmentService
{
    private readonly AutoCountConnectionManager _connectionManager;
    private readonly ILogger<StockAdjustmentService> _logger;

    public StockAdjustmentService(
        AutoCountConnectionManager connectionManager,
        ILogger<StockAdjustmentService> logger)
    {
        _connectionManager = connectionManager;
        _logger = logger;
    }

    public async Task<StockAdjustmentResult> CreateStockAdjustmentAsync(StockAdjustmentInput input)
    {
        return await Task.Run(() =>
        {
            _logger.LogInformation(
                "Creating stock adjustment on {DocDate} with {LineCount} lines",
                input.DocDate,
                input.Lines?.Count ?? 0);

            var result = new StockAdjustmentResult();

            try
            {
                if (input.Lines == null || input.Lines.Count == 0)
                {
                    throw new ArgumentException("At least one line is required for stock adjustment.");
                }

                var dbSetting = _connectionManager.GetDBSetting();
                var userSession = _connectionManager.GetUserSession();

                var cmd = StockAdjustmentCommand.Create(userSession, dbSetting);
                var doc = cmd.AddNew();

                // Header
                if (!DateTime.TryParse(input.DocDate, out var docDate))
                {
                    docDate = DateTime.Today.Date;
                }

                doc.DocDate = docDate.Date;
                doc.Description = string.IsNullOrWhiteSpace(input.Description)
                    ? "Stock adjustment via Lemon Co API"
                    : input.Description;

                if (!string.IsNullOrWhiteSpace(input.RefDocNo))
                {
                    doc.RefDocNo = input.RefDocNo;
                }

                // Details
                foreach (var line in input.Lines)
                {
                    if (string.IsNullOrWhiteSpace(line.ItemCode))
                    {
                        throw new ArgumentException("ItemCode is required for all stock adjustment lines.");
                    }

                    if (line.Quantity == 0)
                    {
                        // Skip zero quantity lines
                        continue;
                    }

                    var dtl = doc.AddDetail();
                    dtl.ItemCode = line.ItemCode;
                    dtl.Qty = line.Quantity;

                    if (line.UnitCost.HasValue)
                    {
                        dtl.UnitCost = line.UnitCost.Value;
                    }
                    else if (line.Quantity > 0)
                    {
                        throw new ArgumentException(
                            $"UnitCost is required when increasing stock for item {line.ItemCode}.");
                    }
                }

                doc.Save();

                result.DocNo = doc.DocNo ?? string.Empty;
                result.Success = true;

                _logger.LogInformation("Stock adjustment created successfully. DocNo: {DocNo}", result.DocNo);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create stock adjustment");
                result.Success = false;
                result.ErrorMessage = ex.Message;
                return result;
            }
        });
    }
}

