using QalatAldhaman.Store.Api.Entities.Enums;

namespace QalatAldhaman.Store.Api.DTOs.Public;

public class OrderLookupResultDto
{
    public string OrderNumber { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public PurchaseMethod PurchaseMethod { get; set; }
    public decimal TotalPriceSnapshot { get; set; }
    public decimal? InstallmentPaymentAmountSnapshot { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ContractPdfUrl { get; set; }
}
