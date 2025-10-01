using LemonCo.Core.Interfaces;
using LemonCo.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AspNetCore.Builder;

public static class AssemblyEndpoints
{
    public static void MapAssemblyEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/assembly")
            .WithTags("Assembly Orders")
            .WithOpenApi();

        // POST /assembly/orders
        app.MapPost("/assembly-orders", async (
            [FromServices] IAssemblyService assemblyService,
            [FromBody] AssemblyOrderInput input) =>
        {
            try
            {
                var order = await assemblyService.CreateAssemblyOrderAsync(input);
                return Results.Created($"/assembly-orders/{order.DocNo}", order);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        })
        .WithName("CreateAssemblyOrder")
        .WithSummary("Create a new assembly order")
        .Produces<AssemblyOrder>(201)
        .Produces(400)
        .Produces(500);

        // GET /assembly-orders/{docNo}
        group.MapGet("/orders/{docNo}", async (
            [FromServices] IAssemblyService assemblyService,
            string docNo) =>
        {
            var order = await assemblyService.GetAssemblyOrderAsync(docNo);
            return order != null ? Results.Ok(order) : Results.NotFound();
        })
        .WithName("GetAssemblyOrder")
        .WithSummary("Get assembly order by document number")
        .Produces<AssemblyOrder>(200)
        .Produces(404);

        // GET /assembly-orders/open
        group.MapGet("/orders/open", async (
            [FromServices] IAssemblyService assemblyService) =>
        {
            var orders = await assemblyService.GetOpenAssemblyOrdersAsync();
            return Results.Ok(orders);
        })
        .WithName("GetOpenAssemblyOrders")
        .WithSummary("Get all open assembly orders")
        .Produces<List<AssemblyOrder>>(200);

        // POST /assemblies/post
        app.MapPost("/assemblies/post", async (
            [FromServices] IAssemblyService assemblyService,
            [FromBody] PostAssemblyInput input) =>
        {
            try
            {
                var result = await assemblyService.PostAssemblyAsync(input);
                return result.Success 
                    ? Results.Ok(result)
                    : Results.BadRequest(result);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        })
        .WithName("PostAssembly")
        .WithSummary("Post assembly order (consume materials, produce finished goods)")
        .Produces<PostAssemblyResult>(200)
        .Produces(400)
        .Produces(500);

        // DELETE /assembly-orders/{docNo}
        group.MapDelete("/orders/{docNo}", async (
            [FromServices] IAssemblyService assemblyService,
            string docNo) =>
        {
            var success = await assemblyService.CancelAssemblyOrderAsync(docNo);
            return success 
                ? Results.Ok(new { message = "Assembly order cancelled" })
                : Results.NotFound();
        })
        .WithName("CancelAssemblyOrder")
        .WithSummary("Cancel an assembly order")
        .Produces(200)
        .Produces(404);
    }
}

