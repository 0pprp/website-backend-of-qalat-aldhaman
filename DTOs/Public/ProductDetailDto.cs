namespace QalatAldhaman.Store.Api.DTOs.Public;

public class ProductDetailDto
{
    public int Id { get; set; }
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
    public List<ProductImagePublicDto> Images { get; set; } = [];
    public CategoryDetailDto Category { get; set; } = null!;
}
