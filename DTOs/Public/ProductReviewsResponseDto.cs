namespace QalatAldhaman.Store.Api.DTOs.Public;

public class ProductReviewsResponseDto
{
    public double AverageRating { get; set; }
    public int TotalApprovedReviews { get; set; }
    public List<ReviewPublicDto> Reviews { get; set; } = [];
}
