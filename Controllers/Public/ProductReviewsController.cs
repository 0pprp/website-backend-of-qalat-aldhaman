using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QalatAldhaman.Store.Api.Data;
using QalatAldhaman.Store.Api.DTOs.Public;
using QalatAldhaman.Store.Api.Entities;

namespace QalatAldhaman.Store.Api.Controllers.Public;

[ApiController]
[Route("api/products/{productId:int}/reviews")]
public class ProductReviewsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductReviewsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Create(int productId, CreateReviewRequestDto request)
    {
        var productActive = await _context.Products.AnyAsync(p => p.Id == productId && p.IsActive);
        if (!productActive)
        {
            return NotFound(new { message = "المنتج غير موجود أو غير متوفر حالياً" });
        }

        if (string.IsNullOrWhiteSpace(request.CustomerName))
        {
            return BadRequest(new { message = "الاسم مطلوب" });
        }

        if (request.Rating < 1 || request.Rating > 5)
        {
            return BadRequest(new { message = "التقييم يجب أن يكون رقماً صحيحاً بين 1 و5" });
        }

        var review = new Review
        {
            ProductId = productId,
            CustomerName = request.CustomerName,
            Rating = request.Rating,
            Comment = request.Comment,
            IsApproved = false,
            CreatedAt = DateTime.UtcNow,
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        return Ok(new { message = "تم استلام رأيك وسيظهر بعد مراجعة الإدارة" });
    }

    [HttpGet]
    public async Task<ActionResult<ProductReviewsResponseDto>> GetApproved(int productId)
    {
        var approvedReviews = await _context.Reviews
            .Where(r => r.ProductId == productId && r.IsApproved)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return Ok(new ProductReviewsResponseDto
        {
            TotalApprovedReviews = approvedReviews.Count,
            AverageRating = approvedReviews.Count > 0 ? Math.Round(approvedReviews.Average(r => r.Rating), 2) : 0,
            Reviews = approvedReviews.Select(r => new ReviewPublicDto
            {
                Id = r.Id,
                CustomerName = r.CustomerName,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,
            }).ToList(),
        });
    }
}
