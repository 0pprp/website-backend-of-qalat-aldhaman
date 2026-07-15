using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QalatAldhaman.Store.Api.Data;
using QalatAldhaman.Store.Api.DTOs.Admin;
using QalatAldhaman.Store.Api.Entities;

namespace QalatAldhaman.Store.Api.Controllers.Admin;

[ApiController]
[Authorize]
[Route("api/admin/products")]
public class AdminProductsController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminProductsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProductDto>>> GetAll([FromQuery] int? categoryId)
    {
        var query = _context.Products.Include(p => p.Images).Include(p => p.Category).AsQueryable();
        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        var products = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();

        return Ok(products.Select(ToDto).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetOne(int id)
    {
        var product = await _context.Products
            .Include(p => p.Images)
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product is null)
        {
            return NotFound(new { message = "المنتج غير موجود" });
        }

        return Ok(ToDto(product));
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(ProductUpsertDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "اسم الموديل مطلوب" });
        }

        var category = await _context.Categories.FindAsync(request.CategoryId);
        if (category is null)
        {
            return BadRequest(new { message = "CategoryId غير موجود" });
        }

        var priceError = ValidatePricesAgainstCategory(request, category);
        if (priceError is not null)
        {
            return BadRequest(new { message = priceError });
        }

        var now = DateTime.UtcNow;
        var product = new Product
        {
            CategoryId = category.Id,
            Name = request.Name,
            Description = request.Description,
            CashPrice = request.CashPrice,
            MonthlyTotalPrice = request.MonthlyTotalPrice,
            MonthlyPaymentAmount = request.MonthlyPaymentAmount,
            DailyTotalPrice = request.DailyTotalPrice,
            DailyPaymentAmount = request.DailyPaymentAmount,
            SKU = request.SKU,
            IsActive = request.IsActive,
            CreatedAt = now,
            UpdatedAt = now,
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        product.Category = category;
        return CreatedAtAction(nameof(GetOne), new { id = product.Id }, ToDto(product));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductDto>> Update(int id, ProductUpsertDto request)
    {
        var product = await _context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);
        if (product is null)
        {
            return NotFound(new { message = "المنتج غير موجود" });
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "اسم الموديل مطلوب" });
        }

        var category = await _context.Categories.FindAsync(request.CategoryId);
        if (category is null)
        {
            return BadRequest(new { message = "CategoryId غير موجود" });
        }

        var priceError = ValidatePricesAgainstCategory(request, category);
        if (priceError is not null)
        {
            return BadRequest(new { message = priceError });
        }

        product.CategoryId = category.Id;
        product.Name = request.Name;
        product.Description = request.Description;
        product.CashPrice = request.CashPrice;
        product.MonthlyTotalPrice = request.MonthlyTotalPrice;
        product.MonthlyPaymentAmount = request.MonthlyPaymentAmount;
        product.DailyTotalPrice = request.DailyTotalPrice;
        product.DailyPaymentAmount = request.DailyPaymentAmount;
        product.SKU = request.SKU;
        product.IsActive = request.IsActive;
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        product.Category = category;
        return Ok(ToDto(product));
    }

    /// <summary>
    /// حذف منتج: حذف نهائي إن لم توجد طلبات مرتبطة به، وإلا تعطيل (IsActive = false) بدل الحذف
    /// (صور المنتج وتقييماته تُحذف تلقائياً مع الحذف النهائي عبر Cascade على مستوى قاعدة البيانات).
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product is null)
        {
            return NotFound(new { message = "المنتج غير موجود" });
        }

        var hasOrders = await _context.Orders.AnyAsync(o => o.ProductId == id);
        if (hasOrders)
        {
            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(new { message = "يوجد طلبات مرتبطة بهذا المنتج، تم تعطيله بدل حذفه نهائياً" });
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/images")]
    public async Task<ActionResult<ProductImageDto>> AddImage(int id, AddProductImageRequestDto request)
    {
        var productExists = await _context.Products.AnyAsync(p => p.Id == id);
        if (!productExists)
        {
            return NotFound(new { message = "المنتج غير موجود" });
        }

        if (string.IsNullOrWhiteSpace(request.ImageUrl))
        {
            return BadRequest(new { message = "ImageUrl مطلوب" });
        }

        var image = new ProductImage
        {
            ProductId = id,
            ImageUrl = request.ImageUrl,
            DisplayOrder = request.DisplayOrder,
        };

        _context.ProductImages.Add(image);
        await _context.SaveChangesAsync();

        return Ok(new ProductImageDto { Id = image.Id, ImageUrl = image.ImageUrl, DisplayOrder = image.DisplayOrder });
    }

    [HttpDelete("{id:int}/images/{imageId:int}")]
    public async Task<IActionResult> DeleteImage(int id, int imageId)
    {
        var image = await _context.ProductImages.FirstOrDefaultAsync(pi => pi.Id == imageId && pi.ProductId == id);
        if (image is null)
        {
            return NotFound(new { message = "الصورة غير موجودة" });
        }

        _context.ProductImages.Remove(image);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static string? ValidatePricesAgainstCategory(ProductUpsertDto request, Category category)
    {
        if (request.CashPrice.HasValue && !category.AllowsCash)
        {
            return "لا يمكن تحديد سعر نقدي (CashPrice) لأن هذه الفئة لا تسمح بالدفع النقدي";
        }

        var hasMonthlyTotal = request.MonthlyTotalPrice.HasValue;
        var hasMonthlyPayment = request.MonthlyPaymentAmount.HasValue;
        if ((hasMonthlyTotal || hasMonthlyPayment) && !category.AllowsMonthlyInstallment)
        {
            return "لا يمكن تحديد سعر التقسيط الشهري لأن هذه الفئة لا تسمح بالتقسيط الشهري";
        }

        if (hasMonthlyTotal != hasMonthlyPayment)
        {
            return "يجب تحديد المبلغ الكلي والدفعة الشهرية معاً بالقسط الشهري (أو تركهما فارغين معاً)";
        }

        var hasDailyTotal = request.DailyTotalPrice.HasValue;
        var hasDailyPayment = request.DailyPaymentAmount.HasValue;
        if ((hasDailyTotal || hasDailyPayment) && !category.AllowsDailyInstallment)
        {
            return "لا يمكن تحديد سعر التقسيط اليومي لأن هذه الفئة لا تسمح بالتقسيط اليومي";
        }

        if (hasDailyTotal != hasDailyPayment)
        {
            return "يجب تحديد المبلغ الكلي والدفعة اليومية معاً بالقسط اليومي (أو تركهما فارغين معاً)";
        }

        return null;
    }

    private static ProductDto ToDto(Product p) => new()
    {
        Id = p.Id,
        CategoryId = p.CategoryId,
        CategoryName = p.Category?.Name ?? string.Empty,
        Name = p.Name,
        Description = p.Description,
        CashPrice = p.CashPrice,
        MonthlyTotalPrice = p.MonthlyTotalPrice,
        MonthlyPaymentAmount = p.MonthlyPaymentAmount,
        DailyTotalPrice = p.DailyTotalPrice,
        DailyPaymentAmount = p.DailyPaymentAmount,
        SKU = p.SKU,
        IsActive = p.IsActive,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt,
        Images = p.Images?
            .OrderBy(i => i.DisplayOrder)
            .Select(i => new ProductImageDto { Id = i.Id, ImageUrl = i.ImageUrl, DisplayOrder = i.DisplayOrder })
            .ToList() ?? [],
    };
}
