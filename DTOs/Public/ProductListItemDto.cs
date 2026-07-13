namespace QalatAldhaman.Store.Api.DTOs.Public;

public class ProductListItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal? CashPrice { get; set; }
    public decimal? MonthlyInstallmentPrice { get; set; }
    public decimal? DailyInstallmentPrice { get; set; }
    public string? ImageUrl { get; set; }
}
