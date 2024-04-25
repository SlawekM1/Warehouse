namespace Warehouse.Models;

public class ProductWarehouse
{
    public object IdOrder { get; set; }
    public int IdProduct { get; set; }
    public int IdWarehouse { get; set; }
    public int Amount { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
    public object? Id { get; set; }
}