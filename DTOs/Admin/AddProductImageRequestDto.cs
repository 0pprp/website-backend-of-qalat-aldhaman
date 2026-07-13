namespace QalatAldhaman.Store.Api.DTOs.Admin;

public class AddProductImageRequestDto
{
    /// <summary>رابط مرفوع مسبقاً عبر POST /api/uploads</summary>
    public string ImageUrl { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}
