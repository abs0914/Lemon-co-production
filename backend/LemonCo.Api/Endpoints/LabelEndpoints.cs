using LemonCo.Core.Interfaces;
using LemonCo.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AspNetCore.Builder;

public static class LabelEndpoints
{
    public static void MapLabelEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/labels")
            .WithTags("Labels & Barcodes")
            .WithOpenApi();

        // POST /labels/print
        group.MapPost("/print", async (
            [FromServices] ILabelService labelService,
            [FromBody] LabelPrintInput input) =>
        {
            try
            {
                var result = await labelService.GenerateLabelAsync(input);
                
                if (result.Success)
                {
                    return Results.Ok(result);
                }
                else
                {
                    return Results.BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        })
        .WithName("PrintLabel")
        .WithSummary("Generate barcode label (ZPL or PDF)")
        .Produces<LabelPrintResult>(200)
        .Produces(400)
        .Produces(500);

        // GET /labels/barcode-config
        group.MapGet("/barcode-config", async (
            [FromServices] ILabelService labelService) =>
        {
            var config = await labelService.GetBarcodeConfigAsync();
            return Results.Ok(config);
        })
        .WithName("GetBarcodeConfig")
        .WithSummary("Get barcode configuration")
        .Produces<BarcodeConfig>(200);

        // PUT /labels/barcode-config
        group.MapPut("/barcode-config", async (
            [FromServices] ILabelService labelService,
            [FromBody] BarcodeConfig config) =>
        {
            var success = await labelService.UpdateBarcodeConfigAsync(config);
            return success
                ? Results.Ok(new { message = "Barcode configuration updated" })
                : Results.BadRequest(new { error = "Failed to update configuration" });
        })
        .WithName("UpdateBarcodeConfig")
        .WithSummary("Update barcode configuration")
        .Produces(200)
        .Produces(400);

        // POST /labels/parse-barcode
        group.MapPost("/parse-barcode", (
            [FromServices] ILabelService labelService,
            [FromBody] ParseBarcodeRequest request) =>
        {
            var (itemCode, quantity) = labelService.ParseBarcodeInput(request.Input);
            return Results.Ok(new { itemCode, quantity });
        })
        .WithName("ParseBarcode")
        .WithSummary("Parse barcode scanner input with quantity separator")
        .Produces(200);
    }

    public record ParseBarcodeRequest(string Input);
}

