using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QalatAldhaman.Store.Api.Data;
using QalatAldhaman.Store.Api.DTOs.Public;
using QalatAldhaman.Store.Api.Entities;
using QalatAldhaman.Store.Api.Entities.Enums;

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
                UsesPackages = c.UsesPackages,
                AllowsCash = c.AllowsCash,
                AllowsMonthlyInstallment = c.AllowsMonthlyInstallment,
                AllowsMonthlyRafidain = c.AllowsMonthlyRafidain,
                AllowsDailyInstallment = c.AllowsDailyInstallment,
                RequiresShopOwner = c.RequiresShopOwner,
                MinInvoiceCash = c.MinInvoiceCash,
                MinInvoiceInstallment = c.MinInvoiceInstallment,
                HasCustomProductField = c.HasCustomProductField,
            })
            .ToListAsync();

        return Ok(categories);
    }

    [HttpGet("{slug}/products")]
    public async Task<ActionResult<List<ProductListItemDto>>> GetProducts(
        string slug, [FromQuery] PurchaseMethod? purchaseMethod, [FromQuery] bool packageOnly = false)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Slug == slug && c.IsActive);
        if (category is null)
        {
            return NotFound(new { message = "الفئة غير موجودة" });
        }

        var query = _context.Products.Where(p => p.CategoryId == category.Id && p.IsActive);
        if (packageOnly)
        {
            query = query.Where(p => p.IsAvailableInPackages);
        }

        var products = await query
            .Include(p => p.Images)
            .OrderBy(p => p.Name)
            .ToListAsync();

        // يُحسب هنا (بعد الجلب) لا داخل Select لأن IsMonthlyInstallmentAvailable/IsDailyInstallmentAvailable
        // خصائص محسوبة بالكود ([NotMapped]) لا يمكن ترجمتها إلى SQL مباشرة.
        var filtered = purchaseMethod switch
        {
            PurchaseMethod.Cash => products.Where(p => p.CashPrice.HasValue),
            PurchaseMethod.MonthlyInstallment => products.Where(p => p.IsMonthlyInstallmentAvailable),
            PurchaseMethod.MonthlyRafidain => products.Where(p => p.IsRafidainInstallmentAvailable),
            PurchaseMethod.DailyInstallment => products.Where(p => p.IsDailyInstallmentAvailable),
            null => products,
            _ => products,
        };

        var result = filtered.Select(p => new ProductListItemDto
        {
            Id = p.Id,
            Name = p.Name,
            CashPrice = p.CashPrice,
            MonthlyTotalPrice = p.IsMonthlyInstallmentAvailable ? p.MonthlyTotalPrice : null,
            MonthlyPaymentAmount = p.IsMonthlyInstallmentAvailable ? p.MonthlyPaymentAmount : null,
            MonthlyDownPayment = p.IsMonthlyInstallmentAvailable ? p.MonthlyDownPayment : null,
            RafidainTotalPrice = p.IsRafidainInstallmentAvailable ? p.RafidainTotalPrice : null,
            RafidainPaymentAmount = p.IsRafidainInstallmentAvailable ? p.RafidainPaymentAmount : null,
            RafidainDownPayment = p.IsRafidainInstallmentAvailable ? p.RafidainDownPayment : null,
            DailyTotalPrice = p.IsDailyInstallmentAvailable ? p.DailyTotalPrice : null,
            DailyPaymentAmount = p.IsDailyInstallmentAvailable ? p.DailyPaymentAmount : null,
            ImageUrl = p.Images.OrderBy(i => i.DisplayOrder).Select(i => i.ImageUrl).FirstOrDefault(),
        }).ToList();

        return Ok(result);
    }

    /// <summary>الباقات الفعّالة لفئة تستخدم نظام الباقات — تُستخدم بشاشة اختيار الباقة قبل اختيار المنتجات.</summary>
    [HttpGet("{slug}/packages")]
    public async Task<ActionResult<List<PackagePublicDto>>> GetPackages(string slug)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Slug == slug && c.IsActive);
        if (category is null)
        {
            return NotFound(new { message = "الفئة غير موجودة" });
        }

        var packages = await _context.Packages
            .Where(p => p.CategoryId == category.Id && p.IsActive)
            .OrderBy(p => p.DisplayOrder)
            .ToListAsync();

        // يُحسب هنا (بعد الجلب) لا داخل Select لأن IsMonthlyInstallmentAvailable/IsDailyInstallmentAvailable
        // خصائص محسوبة بالكود ([NotMapped]) لا يمكن ترجمتها إلى SQL مباشرة — نفس منطق GetProducts بالضبط.
        var result = packages.Select(p => new PackagePublicDto
        {
            Id = p.Id,
            Name = p.Name,
            MinimumTotalPrice = p.MinimumTotalPrice,
            DisplayOrder = p.DisplayOrder,
            CashPrice = p.CashPrice,
            MonthlyTotalPrice = p.IsMonthlyInstallmentAvailable ? p.MonthlyTotalPrice : null,
            MonthlyPaymentAmount = p.IsMonthlyInstallmentAvailable ? p.MonthlyPaymentAmount : null,
            MonthlyDownPayment = p.IsMonthlyInstallmentAvailable ? p.MonthlyDownPayment : null,
            RafidainTotalPrice = p.IsRafidainInstallmentAvailable ? p.RafidainTotalPrice : null,
            RafidainPaymentAmount = p.IsRafidainInstallmentAvailable ? p.RafidainPaymentAmount : null,
            RafidainDownPayment = p.IsRafidainInstallmentAvailable ? p.RafidainDownPayment : null,
            DailyTotalPrice = p.IsDailyInstallmentAvailable ? p.DailyTotalPrice : null,
            DailyPaymentAmount = p.IsDailyInstallmentAvailable ? p.DailyPaymentAmount : null,
        }).ToList();

        return Ok(result);
    }
}
