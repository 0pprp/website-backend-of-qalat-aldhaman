using QalatAldhaman.Store.Api.Entities.Enums;

namespace QalatAldhaman.Store.Api.DTOs.Public;

public class CreateOrderResponseDto
{
    public string OrderNumber { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }

    /// <summary>ملف عقد الشراء الثابت الخاص بموديل المنتج (وليس مولّداً ديناميكياً لكل طلب)</summary>
    public string? ContractPdfUrl { get; set; }
}
