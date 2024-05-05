using cw7.Models;

namespace cw7.DTOs;

public record OrderDTO(int IdProduct, int Amount, DateTime CreatedAt, DateTime FulfilledAt)
{
    public OrderDTO(Order order) : this(order.IdProduct, order.Amount,order.CreatedAt, order.FulfilledAt){}
}

public record OrderDetailsDTO(int IdOrder, int IdProduct, int Amount, DateTime CreatedAt, DateTime FulfilledAt)
{
    public OrderDetailsDTO(Order order) 
        : this(order.IdOrder, order.IdProduct, order.Amount,order.CreatedAt, order.FulfilledAt){}
}
