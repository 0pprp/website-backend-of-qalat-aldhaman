using QalatAldhaman.Store.Api.Entities.Enums;

namespace QalatAldhaman.Store.Api.DTOs.Public;

public class CreateOrderResponseDto
{
    public string OrderNumber { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }

    /// <summary>ملف عقد طريقة الدفع المُختارة بالطلب (مشترك بين كل المنتجات، وليس خاصاً بالمنتج)</summary>
    public string? ContractPdfUrl { get; set; }
}
