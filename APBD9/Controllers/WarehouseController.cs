using APBD9.Models;
using APBD9.Models.DTOs;
using APBD9.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace APBD9.Controllers;

[Route("api/warehouse_product")]
public class WarehouseController : ControllerBase
{
    private readonly IWarehouseService _warehouseService;

    public WarehouseController(IWarehouseService warehouseService)
    {
        _warehouseService = warehouseService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Product>>> GetAllProducts()
    {
        var p = await _warehouseService.GetAllProducts();
        return Ok(p);
    }
    
    [HttpPost]
    public async Task<IActionResult> AddProductWarehouse([FromBody] Product_Warehouse productWarehouse)
    {
        var result = await _warehouseService.insertToProductWarehouse(productWarehouse);
        switch (result)
        {
            case -1: return NotFound("non existent product or warehouse"); 
            case -2: return Conflict("the amount doesn't match");
            case -3: return NotFound("order not found");
            case -4: return Conflict("the order has already been completed");
            default: return Ok(result);
        }
    }

    [HttpPost("procedure")]
    public async Task<IActionResult> AddProductWarehouseProcedure([FromBody] Product_Warehouse productWarehouse)
    {
        var result = await _warehouseService.insertToProductWarehouse(productWarehouse);
        switch (result)
        {
            case -1: return NotFound("non existent product or warehouse"); 
            case -2: return Conflict("the amount doesn't match");
            case -3: return NotFound("order not found");
            case -4: return Conflict("the order has already been completed");
            default: return Ok(result);
        }
    }
}