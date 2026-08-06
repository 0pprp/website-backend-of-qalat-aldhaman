namespace QalatAldhaman.Store.Api.DTOs.Admin;

public class PackageUpsertDto
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal MinimumTotalPrice { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public decimal? CashPrice { get; set; }
    public decimal? MonthlyTotalPrice { get; set; }
    public decimal? MonthlyPaymentAmount { get; set; }
    public decimal? MonthlyDownPayment { get; set; }
    public decimal? RafidainTotalPrice { get; set; }
    public decimal? RafidainPaymentAmount { get; set; }
    public decimal? RafidainDownPayment { get; set; }
    public decimal? DailyTotalPrice { get; set; }
    public decimal? DailyPaymentAmount { get; set; }
}
