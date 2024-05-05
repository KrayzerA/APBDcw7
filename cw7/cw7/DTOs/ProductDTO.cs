using cw7.Models;

namespace cw7.DTOs;

public record ProductDTO(string Name, string Description, Decimal Price)
{
    public ProductDTO(Product product) : this(product.Name, product.Description, product.Price){}
}

public record ProductDetailsDTO(int IdProduct, string Name, string Description, Decimal Price) 
    : ProductDTO(Name, Description, Price)
{
    public ProductDetailsDTO(Product product) 
        : this(product.IdProduct, product.Name, product.Description, product.Price){}
}