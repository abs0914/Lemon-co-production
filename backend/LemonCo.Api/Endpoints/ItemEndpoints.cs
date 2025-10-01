using LemonCo.Core.Interfaces;
using LemonCo.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AspNetCore.Builder;

public static class ItemEndpoints
{
    public static void MapItemEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/items")
            .WithTags("Items & BOM")
            .WithOpenApi();

        // GET /items?search=
        group.MapGet("/", async (
            [FromServices] IItemService itemService,
            [FromQuery] string? search) =>
        {
            var items = await itemService.SearchItemsAsync(search);
            return Results.Ok(items);
        })
        .WithName("SearchItems")
        .WithSummary("Search for items")
        .Produces<List<Item>>(200);

        // GET /items/{itemCode}
        group.MapGet("/{itemCode}", async (
            [FromServices] IItemService itemService,
            string itemCode) =>
        {
            var item = await itemService.GetItemAsync(itemCode);
            return item != null ? Results.Ok(item) : Results.NotFound();
        })
        .WithName("GetItem")
        .WithSummary("Get item by code")
        .Produces<Item>(200)
        .Produces(404);

        // POST /items
        group.MapPost("/", async (
            [FromServices] IItemService itemService,
            [FromBody] Item item) =>
        {
            try
            {
                var created = await itemService.CreateItemAsync(item);
                return Results.Created($"/items/{created.ItemCode}", created);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("CreateItem")
        .WithSummary("Create a new item")
        .Produces<Item>(201)
        .Produces(400);

        // PUT /items/{itemCode}
        group.MapPut("/{itemCode}", async (
            [FromServices] IItemService itemService,
            string itemCode,
            [FromBody] Item item) =>
        {
            try
            {
                item.ItemCode = itemCode;
                var updated = await itemService.UpdateItemAsync(item);
                return Results.Ok(updated);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("UpdateItem")
        .WithSummary("Update an existing item")
        .Produces<Item>(200)
        .Produces(400);
    }
}

