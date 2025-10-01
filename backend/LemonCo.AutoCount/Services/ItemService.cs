using AutoCount.Stock.Item;
using LemonCo.Core.Interfaces;
using LemonCo.Core.Models;
using Microsoft.Extensions.Logging;

namespace LemonCo.AutoCount.Services;

/// <summary>
/// AutoCount implementation of Item service using AutoCount SDK
/// </summary>
public class ItemService : IItemService
{
    private readonly AutoCountConnectionManager _connectionManager;
    private readonly ILogger<ItemService> _logger;

    public ItemService(
        AutoCountConnectionManager connectionManager,
        ILogger<ItemService> logger)
    {
        _connectionManager = connectionManager;
        _logger = logger;
    }

    public async Task<List<Core.Models.Item>> SearchItemsAsync(string? search = null)
    {
        return await Task.Run(() =>
        {
            _logger.LogInformation("Searching items with query: {Search}", search ?? "all");

            var userSession = _connectionManager.GetUserSession();
            var cmd = ItemDataAccess.Create(userSession, userSession.DBSetting);
            var items = new List<Core.Models.Item>();

            // Load all items - filtering would require custom SQL query
            var itemEntities = cmd.LoadAllItem(null);

            // ItemEntities.ItemTable contains the data
            foreach (System.Data.DataRow row in itemEntities.ItemTable.Rows)
            {
                items.Add(new Core.Models.Item
                {
                    ItemCode = row["ItemCode"].ToString() ?? "",
                    Description = row["Description"].ToString() ?? "",
                    BaseUom = row["BaseUOM"].ToString() ?? "",
                    ItemType = row["ItemGroup"].ToString() ?? "GENERAL",
                    HasBom = false // BOM check would require additional query
                });
            }

            return items;
        });
    }

    public async Task<Core.Models.Item?> GetItemAsync(string itemCode)
    {
        return await Task.Run(() =>
        {
            _logger.LogInformation("Getting item: {ItemCode}", itemCode);

            var userSession = _connectionManager.GetUserSession();
            var cmd = ItemDataAccess.Create(userSession, userSession.DBSetting);
            var itemEntity = cmd.LoadItem(itemCode, ItemEntryAction.Edit);

            if (itemEntity == null)
            {
                return null;
            }

            return new Core.Models.Item
            {
                ItemCode = itemEntity.ItemCode,
                Description = itemEntity.Description,
                BaseUom = itemEntity.BaseUomRecord.Uom,
                ItemType = itemEntity.ItemGroup ?? "GENERAL",
                HasBom = false // BOM check would require additional query
            };
        });
    }

    public async Task<Core.Models.Item> CreateItemAsync(Core.Models.Item item)
    {
        return await Task.Run(() =>
        {
            _logger.LogInformation("Creating item: {ItemCode}", item.ItemCode);

            var userSession = _connectionManager.GetUserSession();
            var cmd = ItemDataAccess.Create(userSession, userSession.DBSetting);
            var itemEntity = cmd.NewItem();

            itemEntity.ItemCode = item.ItemCode;
            itemEntity.Description = item.Description;
            itemEntity.ItemGroup = item.ItemType;
            itemEntity.BaseUomRecord.Uom = item.BaseUom;

            cmd.SaveData(itemEntity);

            return item;
        });
    }

    public async Task<Core.Models.Item> UpdateItemAsync(Core.Models.Item item)
    {
        return await Task.Run(() =>
        {
            _logger.LogInformation("Updating item: {ItemCode}", item.ItemCode);

            var userSession = _connectionManager.GetUserSession();
            var cmd = ItemDataAccess.Create(userSession, userSession.DBSetting);
            var itemEntity = cmd.LoadItem(item.ItemCode, ItemEntryAction.Edit);

            if (itemEntity == null)
            {
                throw new ArgumentException($"Item {item.ItemCode} not found");
            }

            itemEntity.Description = item.Description;
            itemEntity.ItemGroup = item.ItemType;

            cmd.SaveData(itemEntity);

            return item;
        });
    }

    public async Task<List<BomLine>> GetBomAsync(string itemCode)
    {
        return await Task.Run(() =>
        {
            _logger.LogInformation("Getting BOM for item: {ItemCode}", itemCode);

            // BOM functionality requires additional AutoCount modules
            // This is a placeholder implementation
            _logger.LogWarning("GetBomAsync not fully implemented - requires BOM module");

            return new List<BomLine>();
        });
    }

    public async Task<bool> SaveBomAsync(string itemCode, List<BomLine> bomLines)
    {
        return await Task.Run(() =>
        {
            _logger.LogInformation("Saving BOM for item: {ItemCode} with {Count} lines",
                itemCode, bomLines.Count);

            // BOM functionality requires additional AutoCount modules
            _logger.LogWarning("SaveBomAsync not fully implemented - requires BOM module");

            return false;
        });
    }

    public async Task<bool> ImportBomFromCsvAsync(string itemCode, string csvContent)
    {
        try
        {
            _logger.LogInformation("Importing BOM from CSV for item: {ItemCode}", itemCode);

            var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var bomLines = new List<BomLine>();

            // Skip header row
            for (int i = 1; i < lines.Length; i++)
            {
                var parts = lines[i].Split(',');
                if (parts.Length >= 3)
                {
                    bomLines.Add(new BomLine
                    {
                        ComponentCode = parts[0].Trim(),
                        QtyPer = decimal.Parse(parts[1].Trim()),
                        Uom = parts[2].Trim(),
                        Description = parts.Length > 3 ? parts[3].Trim() : null,
                        Sequence = i
                    });
                }
            }

            return await SaveBomAsync(itemCode, bomLines);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to import BOM from CSV for item: {ItemCode}", itemCode);
            return false;
        }
    }
}

