namespace QalatAldhaman.Store.Api.DTOs.Admin;

public class CategoryUpsertDto
{
    public string Name { get; set; } = string.Empty;

    /// <summary>اتركه فارغاً ليُولَّد تلقائياً من الاسم</summary>
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public bool AllowsCash { get; set; }
    public bool AllowsMonthlyInstallment { get; set; }
    public bool AllowsMonthlyRafidain { get; set; }
    public bool AllowsDailyInstallment { get; set; }
    public bool RequiresShopOwner { get; set; }
    public decimal? MinInvoiceCash { get; set; }
    public decimal? MinInvoiceInstallment { get; set; }
    public bool HasCustomProductField { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public bool UsesPackages { get; set; }
}
