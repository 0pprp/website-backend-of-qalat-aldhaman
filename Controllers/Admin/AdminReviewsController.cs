using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QalatAldhaman.Store.Api.Data;
using QalatAldhaman.Store.Api.DTOs.Admin;

namespace QalatAldhaman.Store.Api.Controllers.Admin;

[ApiController]
[Authorize]
[Route("api/admin/reviews")]
public class AdminReviewsController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminReviewsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<ReviewDto>>> GetAll([FromQuery] bool? pending, [FromQuery] int? productId)
    {
        var query = _context.Reviews.Include(r => r.Product).AsQueryable();

        if (pending == true)
        {
            query = query.Where(r => !r.IsApproved);
        }

        if (productId.HasValue)
        {
            query = query.Where(r => r.ProductId == productId.Value);
        }

        var reviews = await query
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewDto
            {
                Id = r.Id,
                ProductId = r.ProductId,
                ProductName = r.Product.Name,
                CustomerName = r.CustomerName,
                Rating = r.Rating,
                Comment = r.Comment,
                IsApproved = r.IsApproved,
                CreatedAt = r.CreatedAt,
            })
            .ToListAsync();

        return Ok(reviews);
    }

    [HttpPut("{id:int}/approve")]
    public async Task<ActionResult<ReviewDto>> Approve(int id)
    {
        var review = await _context.Reviews.Include(r => r.Product).FirstOrDefaultAsync(r => r.Id == id);
        if (review is null)
        {
            return NotFound(new { message = "الرأي غير موجود" });
        }

        review.IsApproved = true;
        await _context.SaveChangesAsync();

        return Ok(new ReviewDto
        {
            Id = review.Id,
            ProductId = review.ProductId,
            ProductName = review.Product.Name,
            CustomerName = review.CustomerName,
            Rating = review.Rating,
            Comment = review.Comment,
            IsApproved = review.IsApproved,
            CreatedAt = review.CreatedAt,
        });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var review = await _context.Reviews.FindAsync(id);
        if (review is null)
        {
            return NotFound(new { message = "الرأي غير موجود" });
        }

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
