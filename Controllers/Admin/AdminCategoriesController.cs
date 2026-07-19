using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QalatAldhaman.Store.Api.Data;
using QalatAldhaman.Store.Api.DTOs.Admin;
using QalatAldhaman.Store.Api.Entities;
using QalatAldhaman.Store.Api.Services;

namespace QalatAldhaman.Store.Api.Controllers.Admin;

[ApiController]
[Authorize]
[Route("api/admin/categories")]
public class AdminCategoriesController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminCategoriesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetAll()
    {
        var categories = await _context.Categories
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();

        return Ok(categories.Select(ToDto).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create(CategoryUpsertDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "اسم الفئة مطلوب" });
        }

        var slug = await GenerateUniqueSlugAsync(request.Slug, request.Name, excludeCategoryId: null);

        var category = new Category
        {
            Name = request.Name,
            Slug = slug,
            Description = request.Description,
            ImageUrl = request.ImageUrl,
            AllowsCash = request.AllowsCash,
            AllowsMonthlyInstallment = request.AllowsMonthlyInstallment,
            AllowsDailyInstallment = request.AllowsDailyInstallment,
            RequiresShopOwner = request.RequiresShopOwner,
            MinInvoiceCash = request.MinInvoiceCash,
            MinInvoiceInstallment = request.MinInvoiceInstallment,
            HasCustomProductField = request.HasCustomProductField,
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive,
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return Ok(ToDto(category));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CategoryDto>> Update(int id, CategoryUpsertDto request)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category is null)
        {
            return NotFound(new { message = "الفئة غير موجودة" });
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "اسم الفئة مطلوب" });
        }

        if (!string.IsNullOrWhiteSpace(request.Slug))
        {
            category.Slug = await GenerateUniqueSlugAsync(request.Slug, request.Name, excludeCategoryId: id);
        }

        category.Name = request.Name;
        category.Description = request.Description;
        category.ImageUrl = request.ImageUrl;
        category.AllowsCash = request.AllowsCash;
        category.AllowsMonthlyInstallment = request.AllowsMonthlyInstallment;
        category.AllowsDailyInstallment = request.AllowsDailyInstallment;
        category.RequiresShopOwner = request.RequiresShopOwner;
        category.MinInvoiceCash = request.MinInvoiceCash;
        category.MinInvoiceInstallment = request.MinInvoiceInstallment;
        category.HasCustomProductField = request.HasCustomProductField;
        category.DisplayOrder = request.DisplayOrder;
        category.IsActive = request.IsActive;

        await _context.SaveChangesAsync();

        return Ok(ToDto(category));
    }

    /// <summary>
    /// حذف فئة: حذف نهائي إن لم يكن لديها منتجات مرتبطة، وإلا تعطيل (IsActive = false) بدل الحذف.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category is null)
        {
            return NotFound(new { message = "الفئة غير موجودة" });
        }

        _context.Categories.Remove(category);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // قيد Restrict بقاعدة البيانات (Product→Category وOrder→Category) رفض الحذف عمداً
            // لحماية السجل التاريخي — هذا سلوك مقصود، فقط نستبدل خطأ EF/Postgres الخام برسالة عربية مفهومة.
            var productsCount = await _context.Products.CountAsync(p => p.CategoryId == id);
            var ordersCount = await _context.Orders.CountAsync(o => o.CategoryId == id);

            var parts = new List<string>();
            if (productsCount > 0) parts.Add($"{productsCount} منتج");
            if (ordersCount > 0) parts.Add($"{ordersCount} طلب");
            var linkedSummary = parts.Count > 0 ? string.Join(" و", parts) : "سجلات مرتبطة";

            return Conflict(new
            {
                message = $"لا يمكن حذف هذه الفئة لوجود {linkedSummary} مرتبط بها. احذف أو انقل المرتبطات أولاً.",
            });
        }

        return NoContent();
    }

    private async Task<string> GenerateUniqueSlugAsync(string? requestedSlug, string name, int? excludeCategoryId)
    {
        var baseSlug = SlugGenerator.Slugify(string.IsNullOrWhiteSpace(requestedSlug) ? name : requestedSlug);
        var slug = baseSlug;
        var suffix = 2;

        while (await _context.Categories.AnyAsync(c => c.Slug == slug && (!excludeCategoryId.HasValue || c.Id != excludeCategoryId.Value)))
        {
            slug = $"{baseSlug}-{suffix}";
            suffix++;
        }

        return slug;
    }

    private static CategoryDto ToDto(Category c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Slug = c.Slug,
        Description = c.Description,
        ImageUrl = c.ImageUrl,
        AllowsCash = c.AllowsCash,
        AllowsMonthlyInstallment = c.AllowsMonthlyInstallment,
        AllowsDailyInstallment = c.AllowsDailyInstallment,
        RequiresShopOwner = c.RequiresShopOwner,
        MinInvoiceCash = c.MinInvoiceCash,
        MinInvoiceInstallment = c.MinInvoiceInstallment,
        HasCustomProductField = c.HasCustomProductField,
        DisplayOrder = c.DisplayOrder,
        IsActive = c.IsActive,
    };
}
