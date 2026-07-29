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
}
