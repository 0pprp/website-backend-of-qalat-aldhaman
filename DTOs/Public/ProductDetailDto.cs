namespace QalatAldhaman.Store.Api.DTOs.Public;

public class ProductDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? CashPrice { get; set; }
    public decimal? MonthlyInstallmentPrice { get; set; }
    public decimal? DailyInstallmentPrice { get; set; }
    public string? ContractPdfUrl { get; set; }
    public List<ProductImagePublicDto> Images { get; set; } = [];
    public CategoryDetailDto Category { get; set; } = null!;
}
