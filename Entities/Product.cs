namespace QalatAldhaman.Store.Api.Entities;

public class Product
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    /// <summary>اسم الموديل</summary>
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

    public List<ProductImage> Images { get; set; } = [];
    public List<Review> Reviews { get; set; } = [];
}
