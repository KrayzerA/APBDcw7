using cw7.Services;
using Microsoft.AspNetCore.Mvc;

namespace cw7.Endpoints;

public static class WarehouseEnpoint
{
    public static void RegisterWarehouseEndpoints(this WebApplication app)
    {
        var warehouse = app.MapGroup("warehouse");
        warehouse.MapPost("", AddProductWarehouse);
    }

    private static async Task<IResult> AddProductWarehouse(int idProduct, int idWarehouse, int amount, DateTime date,
        IDbService service, IConfiguration configuration)
    {
        var id = await service.AddWarehouseProduct(idProduct, idWarehouse, amount, date);
        return id is null ? Results.BadRequest() : Results.Ok(id);
    }
}