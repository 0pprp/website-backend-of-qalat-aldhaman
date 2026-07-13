namespace QalatAldhaman.Store.Api.DTOs.Admin;

public class ProductUpsertDto
{
    public int CategoryId { get; set; }

    /// <summary>اسم الموديل</summary>
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? CashPrice { get; set; }
    public decimal? MonthlyInstallmentPrice { get; set; }
    public decimal? DailyInstallmentPrice { get; set; }
    public string? SKU { get; set; }
    public bool IsActive { get; set; } = true;
}
