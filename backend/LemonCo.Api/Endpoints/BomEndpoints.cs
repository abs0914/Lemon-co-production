using LemonCo.Core.Interfaces;
using LemonCo.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AspNetCore.Builder;

public static class BomEndpoints
{
    public static void MapBomEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/boms")
            .WithTags("Items & BOM")
            .WithOpenApi();

        // GET /boms/{itemCode}
        group.MapGet("/{itemCode}", async (
            [FromServices] IItemService itemService,
            string itemCode) =>
        {
            var bom = await itemService.GetBomAsync(itemCode);
            return Results.Ok(bom);
        })
        .WithName("GetBom")
        .WithSummary("Get BOM for an item")
        .Produces<List<BomLine>>(200);

        // POST /boms/{itemCode}
        group.MapPost("/{itemCode}", async (
            [FromServices] IItemService itemService,
            string itemCode,
            [FromBody] List<BomLine> bomLines) =>
        {
            try
            {
                var success = await itemService.SaveBomAsync(itemCode, bomLines);
                return success 
                    ? Results.Ok(new { message = "BOM saved successfully" })
                    : Results.BadRequest(new { error = "Failed to save BOM" });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("SaveBom")
        .WithSummary("Create or update BOM for an item")
        .Produces(200)
        .Produces(400);

        // POST /boms/{itemCode}/import-csv
        group.MapPost("/{itemCode}/import-csv", async (
            [FromServices] IItemService itemService,
            string itemCode,
            [FromBody] CsvImportRequest request) =>
        {
            try
            {
                var success = await itemService.ImportBomFromCsvAsync(itemCode, request.CsvContent);
                return success
                    ? Results.Ok(new { message = "BOM imported successfully" })
                    : Results.BadRequest(new { error = "Failed to import BOM" });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("ImportBomFromCsv")
        .WithSummary("Import BOM from CSV")
        .Produces(200)
        .Produces(400);
    }

    public record CsvImportRequest(string CsvContent);
}

