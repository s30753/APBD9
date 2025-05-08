using System.Threading.Tasks.Dataflow;

namespace APBD9.Models.DTOs;

public class Product_Warehouse
{
    public int IdProduct { get; set; }
    public int IdWarehouse { get; set; }
    public int Amount { get; set; }
    public DateTime CreatedAt { get; set; }
    
}