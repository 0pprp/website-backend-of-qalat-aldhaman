namespace QalatAldhaman.Store.Api.DTOs.Admin;

public class ProductDto
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? CashPrice { get; set; }
    public decimal? MonthlyInstallmentPrice { get; set; }
    public decimal? DailyInstallmentPrice { get; set; }
    public string? ContractPdfUrl { get; set; }
    public string? SKU { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ProductImageDto> Images { get; set; } = [];
}
