namespace QalatAldhaman.Store.Api.DTOs.Public;

public class CategoryPublicDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool UsesPackages { get; set; }

    /// <summary>
    /// معلومات طرق الدفع/القيود — مطلوبة بواجهة اختيار طريقة الدفع لتجربة شراء الباقات (قبل وجود
    /// أي منتج محدَّد بعد)، ومكرَّرة أصلاً بـ CategoryDetailDto المتضمَّن بتفاصيل المنتج.
    /// </summary>
    public bool AllowsCash { get; set; }
    public bool AllowsMonthlyInstallment { get; set; }
    public bool AllowsDailyInstallment { get; set; }
    public bool RequiresShopOwner { get; set; }
    public decimal? MinInvoiceCash { get; set; }
    public decimal? MinInvoiceInstallment { get; set; }
    public bool HasCustomProductField { get; set; }
}
