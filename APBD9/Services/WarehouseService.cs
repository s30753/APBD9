using System.Data;
using APBD9.Models;
using APBD9.Models.DTOs;
using Microsoft.Data.SqlClient;

namespace APBD9.Services;

public class WarehouseService : IWarehouseService
{
    private readonly string _connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Connect Timeout=30;" +
                                                "Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";

    public async Task<List<Product>> GetAllProducts()
    {
        List<Product> products = new List<Product>();
        
        string query = "SELECT * FROM Product";

        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(query, conn))
        {
            await conn.OpenAsync();

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    products.Add(new Product()
                    {
                        IdProduct = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Description = reader.GetString(2),
                        Price = (double)reader.GetDecimal(3),
                    });
                }
            }
        }
        return products;
    }
    public async Task<int> insertToProductWarehouse(Product_Warehouse productWarehouse)
    {

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync();
            
            string command = "SELECT 1 FROM Product WHERE IdProduct = @IdProduct";
            using (SqlCommand cmd = new SqlCommand(command, conn))
            {
                cmd.Parameters.AddWithValue("@IdProduct", productWarehouse.IdProduct);
                var existingProduct = await cmd.ExecuteScalarAsync();
                if (existingProduct == null) return -1;
            }
            
            command = "SELECT 1 FROM Warehouse WHERE IdWarehouse = @IdWarehouse";
            using (SqlCommand cmd = new SqlCommand(command,conn))
            {
                cmd.Parameters.AddWithValue("@IdWarehouse", productWarehouse.IdWarehouse);
                var exisitngWarehouse = await cmd.ExecuteScalarAsync();
                if (exisitngWarehouse == null) return -1;
            }
            
            if (productWarehouse.Amount <= 0) return -2;

            int? idOrder = null;
            
            command = "SELECT IdOrder FROM [Order] WHERE IdProduct = @IdProduct AND Amount = @Amount AND CreatedAt < @CreatedAt";
            using (SqlCommand cmd = new SqlCommand(command, conn))
            {
                cmd.Parameters.AddWithValue("@IdProduct", productWarehouse.IdProduct);
                cmd.Parameters.AddWithValue("@Amount", productWarehouse.Amount);
                cmd.Parameters.AddWithValue("@CreatedAt", productWarehouse.CreatedAt);
                
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (!await reader.ReadAsync())
                    {
                        return -3;
                    }

                    idOrder = reader.GetInt32(0);
                }
            }
            
            command = "SELECT 1 FROM Product_Warehouse WHERE IdOrder = @IdOrder";
            using (SqlCommand cmd = new SqlCommand(command, conn))
            {
                cmd.Parameters.AddWithValue("@IdOrder", idOrder);
                var fulfilledOrder = await cmd.ExecuteScalarAsync();
                if (fulfilledOrder != null) return -4;
            }
            
            command = "UPDATE Order SET FulfilledAt = @Now WHERE IdOrder = @IdOrder";
            using (SqlCommand cmd = new SqlCommand(command, conn))
            {
                cmd.Parameters.AddWithValue("@Now", DateTime.Now);
                cmd.Parameters.AddWithValue("@IdOrder", idOrder);
                await cmd.ExecuteNonQueryAsync();
            }
            
            double finalPrice = 0;
            command = "SELECT Price FROM Product WHERE IdProduct = @IdProduct";
            using (SqlCommand cmd = new SqlCommand(command, conn))
            {
                cmd.Parameters.AddWithValue("@IdProduct", productWarehouse.IdProduct);
                finalPrice = (double)await cmd.ExecuteScalarAsync();
            }
            finalPrice *= productWarehouse.Amount;

            command = @"INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)
VALUES (@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Price, @CreatedAt);
SELECT SCOPE_IDENTITY();";
            using (SqlCommand cmd = new SqlCommand(command, conn))
            {
                cmd.Parameters.AddWithValue("@IdWarehouse", productWarehouse.IdWarehouse);
                cmd.Parameters.AddWithValue("@IdProduct", productWarehouse.IdProduct);
                cmd.Parameters.AddWithValue("@IdOrder", idOrder);
                cmd.Parameters.AddWithValue("@Amount", productWarehouse.Amount);
                cmd.Parameters.AddWithValue("@Price", (decimal)finalPrice);
                cmd.Parameters.AddWithValue("@CreatedAt", productWarehouse.CreatedAt);
                
                var id  = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(id);
            }
        }
    }

    public async Task<int> AddProductToWarehouseAsync(Product_Warehouse productWarehouse)
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("AddProductToWarehouse", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                
                cmd.Parameters.AddWithValue("@IdProduct", productWarehouse.IdProduct);
                cmd.Parameters.AddWithValue("@IdWarehouse", productWarehouse.IdWarehouse);
                cmd.Parameters.AddWithValue("@Amount", productWarehouse.Amount);
                cmd.Parameters.AddWithValue("@CreatedAt", productWarehouse.CreatedAt);
                
                await conn.OpenAsync();
                
                var id = (int)await cmd.ExecuteScalarAsync();
                return id;
            }
        }
        catch (SqlException ex)
        {
            if (ex.Message.Contains("IdProduct does not exist"))
                return -1;
            if (ex.Message.Contains("Provided IdWarehouse does not exist"))
                return -2;
            if (ex.Message.Contains("There is no order to fullfill"))
                return -3;
            return -4;
        }
    }
}