using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QalatAldhaman.Store.Api.Data;
using QalatAldhaman.Store.Api.DTOs.Admin;
using QalatAldhaman.Store.Api.Entities;

namespace QalatAldhaman.Store.Api.Controllers.Admin;

[ApiController]
[Authorize]
[Route("api/admin/packages")]
public class AdminPackagesController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminPackagesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<PackageDto>>> GetAll([FromQuery] int? categoryId)
    {
        var query = _context.Packages.Include(p => p.Category).AsQueryable();
        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        var packages = await query
            .OrderBy(p => p.CategoryId)
            .ThenBy(p => p.DisplayOrder)
            .ToListAsync();

        return Ok(packages.Select(ToDto).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<PackageDto>> Create(PackageUpsertDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "اسم الباقة مطلوب" });
        }

        if (request.MinimumTotalPrice <= 0)
        {
            return BadRequest(new { message = "الحد الأدنى للباقة يجب أن يكون أكبر من صفر" });
        }

        var priceError = ValidatePricePairs(request);
        if (priceError is not null)
        {
            return BadRequest(new { message = priceError });
        }

        var category = await _context.Categories.FindAsync(request.CategoryId);
        if (category is null)
        {
            return BadRequest(new { message = "CategoryId غير موجود" });
        }

        var package = new Package
        {
            CategoryId = category.Id,
            Name = request.Name,
            MinimumTotalPrice = request.MinimumTotalPrice,
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive,
            CashPrice = request.CashPrice,
            MonthlyTotalPrice = request.MonthlyTotalPrice,
            MonthlyPaymentAmount = request.MonthlyPaymentAmount,
            MonthlyDownPayment = request.MonthlyDownPayment,
            RafidainTotalPrice = request.RafidainTotalPrice,
            RafidainPaymentAmount = request.RafidainPaymentAmount,
            RafidainDownPayment = request.RafidainDownPayment,
            DailyTotalPrice = request.DailyTotalPrice,
            DailyPaymentAmount = request.DailyPaymentAmount,
        };

        _context.Packages.Add(package);
        await _context.SaveChangesAsync();

        package.Category = category;
        return Ok(ToDto(package));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PackageDto>> Update(int id, PackageUpsertDto request)
    {
        var package = await _context.Packages.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
        if (package is null)
        {
            return NotFound(new { message = "الباقة غير موجودة" });
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "اسم الباقة مطلوب" });
        }

        if (request.MinimumTotalPrice <= 0)
        {
            return BadRequest(new { message = "الحد الأدنى للباقة يجب أن يكون أكبر من صفر" });
        }

        var priceError = ValidatePricePairs(request);
        if (priceError is not null)
        {
            return BadRequest(new { message = priceError });
        }

        var category = await _context.Categories.FindAsync(request.CategoryId);
        if (category is null)
        {
            return BadRequest(new { message = "CategoryId غير موجود" });
        }

        package.CategoryId = category.Id;
        package.Name = request.Name;
        package.MinimumTotalPrice = request.MinimumTotalPrice;
        package.DisplayOrder = request.DisplayOrder;
        package.IsActive = request.IsActive;
        package.CashPrice = request.CashPrice;
        package.MonthlyTotalPrice = request.MonthlyTotalPrice;
        package.MonthlyPaymentAmount = request.MonthlyPaymentAmount;
        package.MonthlyDownPayment = request.MonthlyDownPayment;
        package.RafidainTotalPrice = request.RafidainTotalPrice;
        package.RafidainPaymentAmount = request.RafidainPaymentAmount;
        package.RafidainDownPayment = request.RafidainDownPayment;
        package.DailyTotalPrice = request.DailyTotalPrice;
        package.DailyPaymentAmount = request.DailyPaymentAmount;

        await _context.SaveChangesAsync();

        package.Category = category;
        return Ok(ToDto(package));
    }

    /// <summary>حذف باقة نهائي — يفشل صراحة (409) إن وُجدت طلبات مرتبطة بها بدل حذف صامت أو تعطيل.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var package = await _context.Packages.FindAsync(id);
        if (package is null)
        {
            return NotFound(new { message = "الباقة غير موجودة" });
        }

        _context.Packages.Remove(package);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            var ordersCount = await _context.Orders.CountAsync(o => o.PackageId == id);
            return Conflict(new
            {
                message = $"لا يمكن حذف هذه الباقة لوجود {ordersCount} طلب مرتبط بها.",
            });
        }

        return NoContent();
    }

    /// <summary>كل زوج (المبلغ الكلي + الدفعة الدورية) يجب أن يُعبَّأ كاملاً أو يُترك فارغاً كاملاً — نفس قاعدة Product بالضبط.</summary>
    private static string? ValidatePricePairs(PackageUpsertDto request)
    {
        if (request.MonthlyTotalPrice.HasValue != request.MonthlyPaymentAmount.HasValue)
        {
            return "يجب تحديد المبلغ الكلي والدفعة الشهرية معاً بالقسط الشهري (أو تركهما فارغين معاً)";
        }

        if (request.RafidainTotalPrice.HasValue != request.RafidainPaymentAmount.HasValue)
        {
            return "يجب تحديد المبلغ الكلي والدفعة الشهرية معاً بقسط الرافدين (أو تركهما فارغين معاً)";
        }

        if (request.DailyTotalPrice.HasValue != request.DailyPaymentAmount.HasValue)
        {
            return "يجب تحديد المبلغ الكلي والدفعة اليومية معاً بالقسط اليومي (أو تركهما فارغين معاً)";
        }

        return null;
    }

    private static PackageDto ToDto(Package p) => new()
    {
        Id = p.Id,
        CategoryId = p.CategoryId,
        CategoryName = p.Category?.Name ?? string.Empty,
        Name = p.Name,
        MinimumTotalPrice = p.MinimumTotalPrice,
        DisplayOrder = p.DisplayOrder,
        IsActive = p.IsActive,
        CashPrice = p.CashPrice,
        MonthlyTotalPrice = p.MonthlyTotalPrice,
        MonthlyPaymentAmount = p.MonthlyPaymentAmount,
        MonthlyDownPayment = p.MonthlyDownPayment,
        RafidainTotalPrice = p.RafidainTotalPrice,
        RafidainPaymentAmount = p.RafidainPaymentAmount,
        RafidainDownPayment = p.RafidainDownPayment,
        DailyTotalPrice = p.DailyTotalPrice,
        DailyPaymentAmount = p.DailyPaymentAmount,
    };
}
