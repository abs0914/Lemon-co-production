using System.Data;
using AutoCount.ARAP.Creditor;
using LemonCo.Core.Interfaces;
using LemonCo.Core.Models;
using Microsoft.Extensions.Logging;

namespace LemonCo.AutoCount.Services;

/// <summary>
/// AutoCount implementation of supplier service using AP Creditor master
/// </summary>
public class SupplierService : ISupplierService
{
    private readonly AutoCountConnectionManager _connectionManager;
    private readonly ILogger<SupplierService> _logger;

    public SupplierService(
        AutoCountConnectionManager connectionManager,
        ILogger<SupplierService> logger)
    {
        _connectionManager = connectionManager;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<List<Supplier>> GetSuppliersAsync(string? search = null)
    {
        return await Task.Run(() =>
        {
            _logger.LogInformation("Loading suppliers from AutoCount with search: {Search}", search ?? "all");

            var dbSetting = _connectionManager.GetDBSetting();

            // Read from AutoCount AP Creditor master table
            const string sql = @"
                SELECT
                    AccNo,
                    CompanyName,
                    Address1,
                    Address2,
                    Address3,
                    Address4,
                    Phone1,
                    Phone2,
                    Attention,
                    EmailAddress
                FROM Creditor";

            var table = dbSetting.GetDataTable(sql, false);
            var suppliers = new List<Supplier>();

            foreach (DataRow row in table.Rows)
            {
                var supplier = new Supplier
                {
                    Code = row["AccNo"]?.ToString() ?? string.Empty,
                    CompanyName = row["CompanyName"]?.ToString() ?? string.Empty,
                    Address1 = row["Address1"]?.ToString(),
                    Address2 = row["Address2"]?.ToString(),
                    Address3 = row["Address3"]?.ToString(),
                    Address4 = row["Address4"]?.ToString(),
                    Phone1 = row["Phone1"]?.ToString(),
                    Phone2 = row["Phone2"]?.ToString(),
                    ContactPerson = row["Attention"]?.ToString(),
                    Email = row["EmailAddress"]?.ToString()
                };

                // Skip empty codes just in case
                if (!string.IsNullOrWhiteSpace(supplier.Code))
                {
                    suppliers.Add(supplier);
                }
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                suppliers = suppliers
                    .Where(s =>
                        (!string.IsNullOrEmpty(s.Code) &&
                         s.Code.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(s.CompanyName) &&
                         s.CompanyName.Contains(term, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            return suppliers;
        });
    }

    /// <inheritdoc />
    public async Task<Supplier?> GetSupplierAsync(string code)
    {
        return await Task.Run(() =>
        {
            _logger.LogInformation("Getting supplier from AutoCount: {Code}", code);

            var userSession = _connectionManager.GetUserSession();
            var cmd = CreditorDataAccess.Create(userSession, userSession.DBSetting);
            var creditor = cmd.GetCreditor(code);

            if (creditor == null)
            {
                return null;
            }

            return new Supplier
            {
                Code = creditor.AccNo,
                CompanyName = creditor.CompanyName,
                Address1 = creditor.Address1,
                Address2 = creditor.Address2,
                Address3 = creditor.Address3,
                Address4 = creditor.Address4,
                Phone1 = creditor.Phone1,
                Phone2 = creditor.Phone2,
                ContactPerson = creditor.Attention,
                Email = creditor.EmailAddress
            };
        });
    }
}

