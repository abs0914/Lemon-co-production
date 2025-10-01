using AutoCount.Invoicing.Sales.SalesOrder;
using AutoCount.ARAP.Debtor;
using LemonCo.Core.Interfaces;
using LemonCo.Core.Models;
using Microsoft.Extensions.Logging;

namespace LemonCo.AutoCount.Services;

/// <summary>
/// AutoCount implementation of Sales Order service using AutoCount SDK
/// </summary>
public class SalesOrderService : ISalesOrderService
{
    private readonly AutoCountConnectionManager _connectionManager;
    private readonly ILogger<SalesOrderService> _logger;

    public SalesOrderService(
        AutoCountConnectionManager connectionManager,
        ILogger<SalesOrderService> logger)
    {
        _connectionManager = connectionManager;
        _logger = logger;
    }

    public async Task<SalesOrderResult> CreateSalesOrderAsync(SalesOrderInput input)
    {
        return await Task.Run(() =>
        {
            _logger.LogInformation("Creating sales order for customer: {CustomerCode}", input.CustomerCode);

            var result = new SalesOrderResult();

            try
            {
                var userSession = _connectionManager.GetUserSession();
                var cmd = SalesOrderCommand.Create(userSession, userSession.DBSetting);
                var doc = cmd.AddNew();

                doc.DebtorCode = input.CustomerCode;
                doc.DocDate = DateTime.Today;
                doc.Description = input.Remarks;

                // Add sales order lines
                foreach (var line in input.Lines)
                {
                    var detail = doc.AddDetail();
                    detail.ItemCode = line.ItemCode;
                    detail.Qty = line.Qty;

                    if (line.UnitPrice.HasValue)
                    {
                        detail.UnitPrice = line.UnitPrice.Value;
                    }

                    if (!string.IsNullOrWhiteSpace(line.Remarks))
                    {
                        detail.Description = line.Remarks;
                    }
                }

                doc.Save();

                result.SoNo = doc.DocNo;
                result.TotalAmount = doc.FinalTotal;
                result.Success = true;
                result.Status = "Created";

                _logger.LogInformation("Sales order created successfully. SO No: {SoNo}, Total: {Total}",
                    doc.DocNo, doc.FinalTotal);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create sales order for customer: {CustomerCode}",
                    input.CustomerCode);
                result.Success = false;
                result.Errors.Add(ex.Message);
                result.Status = "Error";
                return result;
            }
        });
    }

    public async Task<bool> ValidateCustomerAsync(string customerCode)
    {
        return await Task.Run(() =>
        {
            _logger.LogDebug("Validating customer: {CustomerCode}", customerCode);

            var userSession = _connectionManager.GetUserSession();
            var cmd = DebtorDataAccess.Create(userSession, userSession.DBSetting);
            var debtor = cmd.GetDebtor(customerCode);

            return debtor != null;
        });
    }

    public async Task<List<string>> ValidateItemsAsync(List<string> itemCodes)
    {
        return await Task.Run(() =>
        {
            _logger.LogDebug("Validating {Count} items", itemCodes.Count);

            var userSession = _connectionManager.GetUserSession();
            var cmd = global::AutoCount.Stock.Item.ItemDataAccess.Create(userSession, userSession.DBSetting);
            var invalidItems = new List<string>();

            foreach (var itemCode in itemCodes)
            {
                var item = cmd.LoadItem(itemCode, global::AutoCount.Stock.Item.ItemEntryAction.Edit);
                if (item == null)
                {
                    invalidItems.Add(itemCode);
                }
            }

            return invalidItems;
        });
    }
}

