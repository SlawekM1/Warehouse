namespace Warehouse.Models;

public class Order
{
    public object Id { get; set; }
    public DateTime FullfilledAt { get; set; }
    public int IdProduct { get; set; }
    public int Amount { get; set; }
    public DateTime CreatedAt { get; set; }
}