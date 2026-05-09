namespace VehicleParts.API.Models;
using VehicleParts.API.Models;
public class SalesInvoice
{
    public int Id { get; set; }

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public DateTime SaleDate { get; set; } = DateTime.UtcNow;

    public decimal SubTotal { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal GrandTotal { get; set; }

    public bool IsCredit { get; set; }

    public bool IsPaid { get; set; } = true;
}