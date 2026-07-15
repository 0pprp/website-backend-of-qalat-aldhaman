namespace QalatAldhaman.Store.Api.DTOs.Public;

public class ProductListItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal? CashPrice { get; set; }
    public decimal? MonthlyTotalPrice { get; set; }
    public decimal? MonthlyPaymentAmount { get; set; }
    public decimal? DailyTotalPrice { get; set; }
    public decimal? DailyPaymentAmount { get; set; }
    public string? ImageUrl { get; set; }
}
