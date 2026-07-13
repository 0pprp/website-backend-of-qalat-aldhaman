namespace QalatAldhaman.Store.Api.DTOs.Admin;

public class ProductImageDto
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}
