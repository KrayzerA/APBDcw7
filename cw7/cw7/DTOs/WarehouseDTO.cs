using cw7.Models;

namespace cw7.DTOs;

public record WarehouseDTO(string Name, string Address)
{
    public WarehouseDTO(Warehouse warehouse) : this(warehouse.Name, warehouse.Address){}
}

public record WarehouseDetailsDTO(int IdWarehouse, string Name, String Address) : WarehouseDTO(Name, Address)
{
    public WarehouseDetailsDTO(Warehouse warehouse) 
        : this(warehouse.IdWarehouse, warehouse.Name, warehouse.Address){}
}

public record ProductWarehouseDTO(
    int IdWarehouse,
    int IdProduct,
    int IdOrder,
    int Amount,
    Decimal Price,
    DateTime CreatedAt)
{
    public ProductWarehouseDTO(Order order, Warehouse warehouse, Product product, DateTime data) 
        : this(warehouse.IdWarehouse,
            order.IdProduct,
            order.IdOrder,
            order.Amount,
            product.Price * order.Amount,
            data
            )
    {}
}