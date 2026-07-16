using QalatAldhaman.Store.Api.Entities.Enums;

namespace QalatAldhaman.Store.Api.DTOs.Admin;

public class OrderListItemDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int GovernorateId { get; set; }
    public string GovernorateName { get; set; } = string.Empty;
    public PurchaseMethod PurchaseMethod { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalPriceSnapshot { get; set; }
    public decimal? InstallmentPaymentAmountSnapshot { get; set; }
    public DateTime CreatedAt { get; set; }
}
