using QalatAldhaman.Store.Api.Entities.Enums;

namespace QalatAldhaman.Store.Api.DTOs.Admin;

public class RecentOrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public PurchaseMethod PurchaseMethod { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
