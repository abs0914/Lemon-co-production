using LemonCo.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace LemonCo.Data;

/// <summary>
/// Database context for metadata storage
/// </summary>
public class LemonCoDbContext : DbContext
{
    public LemonCoDbContext(DbContextOptions<LemonCoDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<LabelTemplate> LabelTemplates { get; set; }
    public DbSet<AppConfig> AppConfigs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Seed default barcode config
        modelBuilder.Entity<AppConfig>().HasData(
            new AppConfig
            {
                Key = "BarcodeType",
                Value = "Code128",
                Description = "Default barcode type (Code128 or QR)",
                UpdatedAt = DateTime.UtcNow
            },
            new AppConfig
            {
                Key = "BarcodeQtySeparator",
                Value = "*",
                Description = "Quantity separator for barcode scanner input",
                UpdatedAt = DateTime.UtcNow
            },
            new AppConfig
            {
                Key = "BarcodeIncludeQty",
                Value = "false",
                Description = "Whether to include quantity in barcode",
                UpdatedAt = DateTime.UtcNow
            }
        );

        // Seed default ZPL label template
        modelBuilder.Entity<LabelTemplate>().HasData(
            new LabelTemplate
            {
                Id = 1,
                Name = "Default ZPL Label",
                Format = "ZPL",
                TemplateContent = @"^XA
^FO50,50^A0N,50,50^FD{ITEM_CODE}^FS
^FO50,120^A0N,30,30^FD{DESCRIPTION}^FS
^FO50,170^BY3^BCN,100,Y,N,N^FD{BARCODE}^FS
^FO50,290^A0N,25,25^FDBatch: {BATCH_NO}^FS
^FO50,330^A0N,25,25^FDMfg: {MFG_DATE}^FS
^FO50,370^A0N,25,25^FDExp: {EXP_DATE}^FS
^XZ",
                Description = "Default Zebra ZPL label template",
                IsDefault = true,
                CreatedAt = DateTime.UtcNow
            }
        );
    }
}

