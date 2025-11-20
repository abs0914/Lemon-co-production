using LemonCo.Core.Interfaces;
using LemonCo.Core.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Microsoft.AspNetCore.Builder;

public static class SupplierEndpoints
{
    public static void MapSupplierEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/suppliers")
            .WithTags("Suppliers")
            .WithOpenApi()
            .RequireAuthorization();

        // GET /suppliers?search=
        group.MapGet("/", async (
            [FromServices] ISupplierService supplierService,
            [FromQuery] string? search) =>
        {
            var suppliers = await supplierService.GetSuppliersAsync(search);
            return Results.Ok(suppliers);
        })
        .WithName("GetSuppliers")
        .WithSummary("Get suppliers (AutoCount AP creditors)")
        .Produces<List<Supplier>>(200);

        // GET /suppliers/{code}
        group.MapGet("/{code}", async (
            [FromServices] ISupplierService supplierService,
            string code) =>
        {
            var supplier = await supplierService.GetSupplierAsync(code);
            return supplier != null ? Results.Ok(supplier) : Results.NotFound();
        })
        .WithName("GetSupplier")
        .WithSummary("Get supplier by code")
        .Produces<Supplier>(200)
        .Produces(404);

        // Endpoint used by Supabase Edge Functions for AutoCount sync
        var acGroup = app.MapGroup("/autocount/suppliers")
            .WithTags("Suppliers")
            .WithOpenApi()
            .RequireAuthorization();

        acGroup.MapGet("/", async ([FromServices] ISupplierService supplierService) =>
        {
            var suppliers = await supplierService.GetSuppliersAsync(null);

            var result = suppliers.Select(s => new
            {
                code = s.Code,
                companyName = s.CompanyName,
                contactPerson = s.ContactPerson,
                phone = s.Phone1 ?? s.Phone2,
                email = s.Email,
                address = string.Join(", ",
                    new[] { s.Address1, s.Address2, s.Address3, s.Address4 }
                        .Where(a => !string.IsNullOrWhiteSpace(a))),
                creditTerms = (int?)null,
                isActive = true
            });

            return Results.Ok(result);
        })
        .WithName("GetAutoCountSuppliers")
        .WithSummary("Get suppliers for AutoCount sync (Supabase functions)");
    }
}

