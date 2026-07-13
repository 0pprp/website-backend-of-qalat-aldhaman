namespace QalatAldhaman.Store.Api.DTOs.Public;

public class CreateReviewRequestDto
{
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>1-5</summary>
    public int Rating { get; set; }
    public string? Comment { get; set; }
}
