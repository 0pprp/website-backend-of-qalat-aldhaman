using QalatAldhaman.Store.Api.Entities.Enums;

namespace QalatAldhaman.Store.Api.Entities;

/// <summary>ملف عقد/وصل أمانة واحد لكل طريقة دفع — مشترك بين كل المنتجات ضمن نفس الطريقة.</summary>
public class PurchaseMethodContract
{
    public int Id { get; set; }
    public PurchaseMethod PurchaseMethod { get; set; }
    public string? ContractPdfUrl { get; set; }
    public DateTime UpdatedAt { get; set; }
}
