namespace QalatAldhaman.Store.Api.DTOs.Public;

public class PackagePublicDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal MinimumTotalPrice { get; set; }
    public int DisplayOrder { get; set; }
}
