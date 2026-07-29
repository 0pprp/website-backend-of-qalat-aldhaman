namespace QalatAldhaman.Store.Api.DTOs.Public;

/// <summary>
/// معلومات الفئة اللازمة للفرونت اند لبناء فورم الطلب الديناميكي (تُضمَّن داخل تفاصيل المنتج).
/// </summary>
public class CategoryDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public bool AllowsCash { get; set; }
    public bool AllowsMonthlyInstallment { get; set; }
    public bool AllowsDailyInstallment { get; set; }
    public bool RequiresShopOwner { get; set; }
    public decimal? MinInvoiceCash { get; set; }
    public decimal? MinInvoiceInstallment { get; set; }
    public bool HasCustomProductField { get; set; }
    public bool UsesPackages { get; set; }
}
