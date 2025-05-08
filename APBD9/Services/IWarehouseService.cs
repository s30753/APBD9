using APBD9.Models;
using APBD9.Models.DTOs;

namespace APBD9.Services;

public interface IWarehouseService
{
    Task<int> insertToProductWarehouse(Product_Warehouse productWarehouse);
    Task<List<Product>> GetAllProducts();
    Task<int> AddProductToWarehouseAsync(Product_Warehouse productWarehouse);
}