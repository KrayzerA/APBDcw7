using System.Data;
using System.Data.SqlClient;
using System.Transactions;
using cw7.DTOs;
using cw7.Models;
using Dapper;

namespace cw7.Services;

public interface IDbService
{
    Task<Product?> GetProduct(int id);
    Task<Order?> GetOrder(int id);
    Task<Warehouse?> GetWarehouse(int id);
    Task<Order?> GetOrderWithProduct(int idProduct, int amount, DateTime data);
    Task<int?> AddWarehouseProduct(int idProduct, int idWarehouse, int amount, DateTime date);
    Task<int?> AddWarehouseProductAsync(int idProduct, int idWarehouse, int amount, DateTime date);
}

public class DbService(IConfiguration configuration) : IDbService
{

    private async Task<SqlConnection> GetConnection()
    {
        var connection = new SqlConnection(configuration.GetConnectionString("Default"));
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        return connection;
    }
    
    public async Task<Product?> GetProduct(int id)
    {
        await using var connection = await GetConnection();

        var com = new SqlCommand("SELECT p.Description, p.Name, p.Price FROM Product p Where IdProduct = @1", connection);
        com.Parameters.AddWithValue("@1", id);
        var reader = await com.ExecuteReaderAsync();
        if (!reader.HasRows)
        {
            return null;
        }

        await reader.ReadAsync();

        return new Product
        {
            IdProduct = id,
            Description = reader.GetString(0),
            Name = reader.GetString(1),
            Price = reader.GetDecimal(2)
        };
    }

    public async Task<Order?> GetOrder(int id)
    {
        await using var connection = await GetConnection();

        var com = new SqlCommand("Select o.IdProduct, o.Amount, o.CreatedAt, o.FulfilledAt FROM [Order] o where IdOrder = @1", connection);
        com.Parameters.AddWithValue("@1", id);
        var reader = await com.ExecuteReaderAsync();
        if (!reader.HasRows)
        {
            return null;
        }

        await reader.ReadAsync();
        
        return new Order
        {
            IdOrder = id,
            IdProduct = reader.GetInt32(0),
            Amount = reader.GetInt32(1),
            CreatedAt = reader.GetDateTime(2),
            FulfilledAt = reader.GetDateTime(3)
        };
    }

    public async Task<Warehouse?> GetWarehouse(int id)
    {
        await using var connection = await GetConnection();

        var com = new SqlCommand("select w.Name, w.Address from Warehouse w where IdWarehouse = @1", connection);
        com.Parameters.AddWithValue("@1", id);
        var reader = await com.ExecuteReaderAsync();
        if (!reader.HasRows)
        {
            return null;
        }

        await reader.ReadAsync();
        return new Warehouse
        {
            IdWarehouse = id,
            Name = reader.GetString(0),
            Address = reader.GetString(1)
        };
    }

    public async Task<Order?> GetOrderWithProduct(int idProduct, int amount, DateTime data)
    {
        await using var connection = await GetConnection();

        var com = new SqlCommand("Select o.IdOrder, o.IdProduct, o.Amount, o.CreatedAt, o.FulfilledAt FROM [Order] o " +
                                 "Where o.IdProduct = @1 and o.Amount = @2 and CreatedAt < @3 and FulfilledAt IS NULL", connection);
        com.Parameters.AddWithValue("@1", idProduct);
        com.Parameters.AddWithValue("@2", amount);
        com.Parameters.AddWithValue("@3", data);
        var reader = await com.ExecuteReaderAsync();
        if (!reader.HasRows)
        {
            return null;
        }

        await reader.ReadAsync();
        return new Order
        {
            IdOrder = reader.GetInt32(0),
            IdProduct = reader.GetInt32(1),
            Amount = reader.GetInt32(2),
            CreatedAt = reader.GetDateTime(3)
        };
    }

    private async Task<Boolean> CheckProductWarehouse(int idOrder)
    {
        await using var connection = await GetConnection();

        var com = new SqlCommand("select pw.IdProductWarehouse from Product_Warehouse pw where IdOrder = @1", connection);
        com.Parameters.AddWithValue("@1", idOrder);
        var reader = await com.ExecuteReaderAsync();
        
        return reader.HasRows;
    }

    public async Task<int?> AddWarehouseProduct(int idProduct, int idWarehouse, int amount, DateTime date)
    {
        if (amount <= 0)
        {
            return null;
        }
        
        
        var product = await GetProduct(idProduct);
        if (product is null)
        {
            return null;
        }
        
        var warehouse = await GetWarehouse(idWarehouse);
        if (warehouse is null)
        {
            return null;
        }

        var order = await GetOrderWithProduct(idProduct, amount, date);
        if (order is null)
        {
            return null;
        }

        if (await CheckProductWarehouse(order.IdOrder))
        {
            return null;
        }
        
        await using var connection = await GetConnection();
        var tran = await connection.BeginTransactionAsync();
        
        try
        {
            var com = new SqlCommand("INSERT INTO Product_Warehouse VALUES (@1,@2,@3,@4,@5,@6);" +
                                     "SELECT CAST(scope_identity() as int)", connection, (SqlTransaction)tran);
            com.Parameters.AddWithValue("@1", idWarehouse);
            com.Parameters.AddWithValue("@2", idProduct);
            com.Parameters.AddWithValue("@3", order.IdOrder);
            com.Parameters.AddWithValue("@4", amount);
            com.Parameters.AddWithValue("@5", amount * product.Price);
            com.Parameters.AddWithValue("@6", DateTime.Now);

            var id = (int)(await com.ExecuteScalarAsync())!;
            
            com.Parameters.Clear();
            com.CommandText = "UPDATE [Order] SET FulfilledAt = @1 WHERE IdOrder = @2";
            com.Parameters.AddWithValue("@1", DateTime.Now);
            com.Parameters.AddWithValue("@2", order.IdOrder);
            var affected = await com.ExecuteNonQueryAsync();
            if (affected == 0)
            {
                await tran.RollbackAsync();
                return null;
            }

            await tran.CommitAsync();
            return id;
        }
        catch (Exception)
        {
            await tran.RollbackAsync();
            return null;
        }
    }

    public async Task<int?> AddWarehouseProductAsync(int idProduct, int idWarehouse, int amount, DateTime date)
    {
        if (amount <= 0)
        {
            return null;
        }
        
        await using var connection = await GetConnection();
        var tran = await connection.BeginTransactionAsync();

        try
        {
            var com = new SqlCommand("SELECT p.Name, p.Description, p.Price FROM Product p WHERE IdProduct = @1",
                connection, (SqlTransaction)tran);
            com.Parameters.AddWithValue("@1", idProduct);

            var reader = await com.ExecuteReaderAsync();
            if (!reader.HasRows) return null;

            await reader.ReadAsync();
            var product = new Product
            {
                IdProduct = idProduct,
                Name = reader.GetString(0),
                Description = reader.GetString(1),
                Price = reader.GetDecimal(2)
            };

            com.Parameters.Clear();
            com.CommandText = "SELECT w.Name, w.Address FROM Warehouse w WHERE IdWarehouse = @1";
            com.Parameters.AddWithValue("@1", idWarehouse);
            
            if (!reader.IsClosed) await reader.CloseAsync();
            reader = await com.ExecuteReaderAsync();
            if (!reader.HasRows) return null;

            await reader.ReadAsync();
            var warehouse = new Warehouse
            {
                IdWarehouse = idWarehouse,
                Name = reader.GetString(0),
                Address = reader.GetString(1)
            };
            
            com.Parameters.Clear();
            com.CommandText = "Select o.IdOrder, o.IdProduct, o.Amount, o.CreatedAt, o.FulfilledAt FROM [Order] o " +
                              "WHERE IdProduct = @1 and Amount = @2 and CreatedAt < @3 and FulfilledAt IS NULL";
            com.Parameters.AddWithValue("@1",idProduct);
            com.Parameters.AddWithValue("@2",amount);
            com.Parameters.AddWithValue("@3",date);
            
            if (!reader.IsClosed) await reader.CloseAsync();
            reader = await com.ExecuteReaderAsync();
            if (!reader.HasRows) return null;
            
            await reader.ReadAsync();
            var order = new Order
            {
                IdOrder = reader.GetInt32(0),
                IdProduct = reader.GetInt32(1),
                Amount = reader.GetInt32(2),
                CreatedAt = reader.GetDateTime(3)

            };
            com.Parameters.Clear();
            com.CommandText = "SELECT 1 FROM Product_Warehouse WHERE IdOrder = @1";
            com.Parameters.AddWithValue("@1", order.IdOrder);
            
            if (!reader.IsClosed) await reader.CloseAsync();
            reader = await com.ExecuteReaderAsync();
            if (reader.HasRows) return null;
            
            com.Parameters.Clear();
            com.CommandText = "UPDATE [Order] SET FulfilledAt = @1 WHERE IdOrder = @2";
            com.Parameters.AddWithValue("@1", DateTime.Now);
            com.Parameters.AddWithValue("@2", order.IdOrder);
            
            if (!reader.IsClosed) await reader.CloseAsync();
            var affected = await com.ExecuteNonQueryAsync();
            if (affected == 0) return null;
            
            com.Parameters.Clear();
            com.CommandText = "INSERT INTO Product_Warehouse VALUES (@1,@2,@3,@4,@5,@6);" +
                              "SELECT CAST(scope_identity() as int)";
            com.Parameters.AddWithValue("@1", idWarehouse);
            com.Parameters.AddWithValue("@2", idProduct);
            com.Parameters.AddWithValue("@3", order.IdOrder);
            com.Parameters.AddWithValue("@4", amount);
            com.Parameters.AddWithValue("@5", amount * product.Price);
            com.Parameters.AddWithValue("@6", DateTime.Now);
            
            if (!reader.IsClosed) await reader.CloseAsync();
            var id = (int)(await com.ExecuteScalarAsync())!;
            
            await reader.CloseAsync();
            await tran.CommitAsync();
            return id;

        }
        catch (Exception)
        {
            await tran.RollbackAsync();
            throw;
        }
    }
}