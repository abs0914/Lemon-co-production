using LemonCo.Core.Interfaces;
using LemonCo.Core.Models;
using LemonCo.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace LemonCo.AutoCount.Services;

/// <summary>
/// Service for generating barcode labels
/// </summary>
public class LabelService : ILabelService
{
    private readonly IItemService _itemService;
    private readonly LemonCoDbContext _dbContext;
    private readonly ILogger<LabelService> _logger;

    public LabelService(
        IItemService itemService,
        LemonCoDbContext dbContext,
        ILogger<LabelService> logger)
    {
        _itemService = itemService;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<LabelPrintResult> GenerateLabelAsync(LabelPrintInput input)
    {
        try
        {
            _logger.LogInformation("Generating {Format} label for item: {ItemCode}", 
                input.Format, input.ItemCode);

            // Get item details
            var item = await _itemService.GetItemAsync(input.ItemCode);
            if (item == null)
            {
                return new LabelPrintResult
                {
                    Success = false,
                    ErrorMessage = $"Item {input.ItemCode} not found"
                };
            }

            // Get label template
            var template = await _dbContext.LabelTemplates
                .Where(t => t.Format == input.Format && t.IsDefault)
                .FirstOrDefaultAsync();

            if (template == null)
            {
                return new LabelPrintResult
                {
                    Success = false,
                    ErrorMessage = $"No default {input.Format} template found"
                };
            }

            string content;
            string contentType;

            if (input.Format.ToUpper() == "ZPL")
            {
                content = GenerateZplLabel(template.TemplateContent, item, input);
                contentType = "text/plain";
            }
            else // PDF
            {
                content = await GeneratePdfLabelAsync(item, input);
                contentType = "application/pdf";
            }

            return new LabelPrintResult
            {
                Success = true,
                Content = content,
                ContentType = contentType
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate label for item: {ItemCode}", input.ItemCode);
            return new LabelPrintResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    private string GenerateZplLabel(string template, Item item, LabelPrintInput input)
    {
        // Replace placeholders in ZPL template
        var zpl = template
            .Replace("{ITEM_CODE}", item.ItemCode)
            .Replace("{DESCRIPTION}", item.Description)
            .Replace("{BARCODE}", item.Barcode ?? item.ItemCode)
            .Replace("{BATCH_NO}", input.BatchNo ?? "N/A")
            .Replace("{MFG_DATE}", input.MfgDate ?? DateTime.Today.ToString("yyyy-MM-dd"))
            .Replace("{EXP_DATE}", input.ExpDate ?? DateTime.Today.AddMonths(6).ToString("yyyy-MM-dd"));

        // If multiple copies, repeat the label
        if (input.Copies > 1)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < input.Copies; i++)
            {
                sb.AppendLine(zpl);
            }
            return sb.ToString();
        }

        return zpl;
    }

    private async Task<string> GeneratePdfLabelAsync(Item item, LabelPrintInput input)
    {
        // In a real implementation, this would use a PDF library like QuestPDF or iTextSharp
        // to generate a proper PDF with barcode
        
        // For now, return a base64 encoded placeholder
        await Task.Delay(50);

        var pdfContent = $@"PDF Label for {item.ItemCode}
Description: {item.Description}
Barcode: {item.Barcode ?? item.ItemCode}
Batch: {input.BatchNo ?? "N/A"}
Mfg Date: {input.MfgDate ?? DateTime.Today.ToString("yyyy-MM-dd")}
Exp Date: {input.ExpDate ?? DateTime.Today.AddMonths(6).ToString("yyyy-MM-dd")}";

        var bytes = Encoding.UTF8.GetBytes(pdfContent);
        return Convert.ToBase64String(bytes);
    }

    public async Task<BarcodeConfig> GetBarcodeConfigAsync()
    {
        var barcodeType = await _dbContext.AppConfigs
            .Where(c => c.Key == "BarcodeType")
            .Select(c => c.Value)
            .FirstOrDefaultAsync() ?? "Code128";

        var qtySeparator = await _dbContext.AppConfigs
            .Where(c => c.Key == "BarcodeQtySeparator")
            .Select(c => c.Value)
            .FirstOrDefaultAsync() ?? "*";

        var includeQty = await _dbContext.AppConfigs
            .Where(c => c.Key == "BarcodeIncludeQty")
            .Select(c => c.Value)
            .FirstOrDefaultAsync() ?? "false";

        return new BarcodeConfig
        {
            Type = barcodeType,
            QtySeparator = qtySeparator,
            IncludeQty = bool.Parse(includeQty)
        };
    }

    public async Task<bool> UpdateBarcodeConfigAsync(BarcodeConfig config)
    {
        try
        {
            var configs = await _dbContext.AppConfigs
                .Where(c => c.Key.StartsWith("Barcode"))
                .ToListAsync();

            foreach (var cfg in configs)
            {
                switch (cfg.Key)
                {
                    case "BarcodeType":
                        cfg.Value = config.Type;
                        break;
                    case "BarcodeQtySeparator":
                        cfg.Value = config.QtySeparator;
                        break;
                    case "BarcodeIncludeQty":
                        cfg.Value = config.IncludeQty.ToString().ToLower();
                        break;
                }
                cfg.UpdatedAt = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update barcode configuration");
            return false;
        }
    }

    public (string itemCode, decimal quantity) ParseBarcodeInput(string input)
    {
        var config = GetBarcodeConfigAsync().Result;
        
        if (input.Contains(config.QtySeparator))
        {
            var parts = input.Split(config.QtySeparator);
            if (parts.Length == 2 && decimal.TryParse(parts[1], out var qty))
            {
                return (parts[0], qty);
            }
        }

        return (input, 1m);
    }
}

