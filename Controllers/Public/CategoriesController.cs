using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QalatAldhaman.Store.Api.Data;
using QalatAldhaman.Store.Api.DTOs.Public;

namespace QalatAldhaman.Store.Api.Controllers.Public;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _context;

    public CategoriesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<CategoryPublicDto>>> GetAll()
    {
        var categories = await _context.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new CategoryPublicDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                Description = c.Description,
                ImageUrl = c.ImageUrl,
                DisplayOrder = c.DisplayOrder,
            })
            .ToListAsync();

        return Ok(categories);
    }

    [HttpGet("{slug}/products")]
    public async Task<ActionResult<List<ProductListItemDto>>> GetProducts(string slug)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Slug == slug && c.IsActive);
        if (category is null)
        {
            return NotFound(new { message = "الفئة غير موجودة" });
        }

        var products = await _context.Products
            .Where(p => p.CategoryId == category.Id && p.IsActive)
            .Include(p => p.Images)
            .OrderBy(p => p.Name)
            .Select(p => new ProductListItemDto
            {
                Id = p.Id,
                Name = p.Name,
                CashPrice = p.CashPrice,
                MonthlyInstallmentPrice = p.MonthlyInstallmentPrice,
                DailyInstallmentPrice = p.DailyInstallmentPrice,
                ImageUrl = p.Images.OrderBy(i => i.DisplayOrder).Select(i => i.ImageUrl).FirstOrDefault(),
            })
            .ToListAsync();

        return Ok(products);
    }
}
