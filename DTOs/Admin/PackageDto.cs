namespace QalatAldhaman.Store.Api.DTOs.Admin;

public class PackageDto
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal MinimumTotalPrice { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
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
