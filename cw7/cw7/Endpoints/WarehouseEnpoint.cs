using cw7.DTOs;
using cw7.Services;
using cw7.Validators;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace cw7.Endpoints;

public static class WarehouseEnpoint
{
    public static void RegisterWarehouseEndpoints(this WebApplication app)
    {
        var warehouse = app.MapGroup("warehouse");
        warehouse.MapPost("", AddProductWarehouse);
    }

    private static async Task<IResult> AddProductWarehouse(RequestDTO request,
        IDbService service, IConfiguration configuration, RequestValidator validator)
    {
        var validate = await validator.ValidateAsync(request);
        if (!validate.IsValid)
        {
            return Results.ValidationProblem(validate.ToDictionary());
        }
        var product = await service.GetProduct(request.IdProduct);
        if (product is null)
        {
            return Results.NotFound($"No such product with id: {request.IdProduct}");
        }
        
        var warehouse = await service.GetWarehouse(request.IdWarehouse);
        if (warehouse is null)
        {
            return Results.NotFound($"No such warehouse with id:{request.IdWarehouse}");
        }
        var order = await service.GetOrderWithProduct(request.IdProduct, request.Amount, request.CreatedAt);
        if (order is null)
        {
            return Results.NotFound($"No such order with product {product.Name} and amount {request.Amount}");
        }
        
        var id = await service.AddWarehouseProduct(product,warehouse,order);
        // var id = await service.AddProductToWarehouse(idProduct, idWarehouse, amount, date);
        // var id = await service.AddWarehouseProductAsync(request.IdProduct, request.IdWarehouse, request.Amount, request.CreatedAt);
        return id is null ? Results.BadRequest() : Results.Ok(id);
    }
}