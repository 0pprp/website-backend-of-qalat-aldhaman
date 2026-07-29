namespace QalatAldhaman.Store.Api.DTOs.Admin;

public class PackageUpsertDto
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal MinimumTotalPrice { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
