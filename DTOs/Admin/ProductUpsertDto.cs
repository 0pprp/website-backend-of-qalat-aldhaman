namespace QalatAldhaman.Store.Api.DTOs.Admin;

public class ProductUpsertDto
{
    public int CategoryId { get; set; }

    /// <summary>اسم الموديل</summary>
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? CashPrice { get; set; }
    public decimal? MonthlyTotalPrice { get; set; }
    public decimal? MonthlyPaymentAmount { get; set; }
    public decimal? MonthlyDownPayment { get; set; }
    public decimal? RafidainTotalPrice { get; set; }
    public decimal? RafidainPaymentAmount { get; set; }
    public decimal? RafidainDownPayment { get; set; }
    public decimal? DailyTotalPrice { get; set; }
    public decimal? DailyPaymentAmount { get; set; }
    public string? SKU { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsAvailableInPackages { get; set; }
}
