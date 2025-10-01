using LemonCo.Core.Interfaces;
using LemonCo.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AspNetCore.Builder;

public static class SalesOrderEndpoints
{
    public static void MapSalesOrderEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/sales-orders")
            .WithTags("Sales Orders (Integration)")
            .WithOpenApi();

        // POST /sales-orders
        group.MapPost("/", async (
            [FromServices] ISalesOrderService salesOrderService,
            [FromBody] SalesOrderInput input) =>
        {
            try
            {
                var result = await salesOrderService.CreateSalesOrderAsync(input);
                
                if (result.Success)
                {
                    return Results.Created($"/sales-orders/{result.SoNo}", result);
                }
                else
                {
                    return Results.BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: 500,
                    title: "Internal Server Error"
                );
            }
        })
        .WithName("CreateSalesOrder")
        .WithSummary("Create sales order from external platform")
        .WithDescription("Integration endpoint for external order management platforms to push sales orders into AutoCount")
        .Produces<SalesOrderResult>(201)
        .Produces<SalesOrderResult>(400)
        .Produces(500);

        // GET /sales-orders/validate-customer/{customerCode}
        group.MapGet("/validate-customer/{customerCode}", async (
            [FromServices] ISalesOrderService salesOrderService,
            string customerCode) =>
        {
            var isValid = await salesOrderService.ValidateCustomerAsync(customerCode);
            return Results.Ok(new { customerCode, isValid });
        })
        .WithName("ValidateCustomer")
        .WithSummary("Validate customer exists in AutoCount")
        .Produces(200);

        // POST /sales-orders/validate-items
        group.MapPost("/validate-items", async (
            [FromServices] ISalesOrderService salesOrderService,
            [FromBody] List<string> itemCodes) =>
        {
            var invalidItems = await salesOrderService.ValidateItemsAsync(itemCodes);
            return Results.Ok(new 
            { 
                allValid = !invalidItems.Any(),
                invalidItems 
            });
        })
        .WithName("ValidateItems")
        .WithSummary("Validate items exist in AutoCount")
        .Produces(200);
    }
}

