using LemonCo.Core.Interfaces;
using LemonCo.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AspNetCore.Builder;

public static class StockAdjustmentEndpoints
{
    public static void MapStockAdjustmentEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/stock-adjustments")
            .WithTags("Stock Adjustments")
            .WithOpenApi()
            .RequireAuthorization();

        // POST /stock-adjustments
        group.MapPost("/", async (
            [FromServices] IStockAdjustmentService stockAdjustmentService,
            [FromBody] StockAdjustmentInput input) =>
        {
            try
            {
                var result = await stockAdjustmentService.CreateStockAdjustmentAsync(input);

                if (result.Success)
                {
                    return Results.Created($"/stock-adjustments/{result.DocNo}", result);
                }

                return Results.BadRequest(result);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        })
        .WithName("CreateStockAdjustment")
        .WithSummary("Create a stock adjustment document (increase/decrease stock directly)")
        .Produces<StockAdjustmentResult>(201)
        .Produces<StockAdjustmentResult>(400)
        .Produces(500);
    }
}

