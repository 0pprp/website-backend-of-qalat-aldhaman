using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QalatAldhaman.Store.Api.Data;
using QalatAldhaman.Store.Api.DTOs.Public;

namespace QalatAldhaman.Store.Api.Controllers.Public;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDetailDto>> GetOne(int id)
    {
        var product = await _context.Products
            .Include(p => p.Images)
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive && p.Category.IsActive);

        if (product is null)
        {
            return NotFound(new { message = "المنتج غير موجود" });
        }

        var dto = new ProductDetailDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            CashPrice = product.CashPrice,
            MonthlyTotalPrice = product.IsMonthlyInstallmentAvailable ? product.MonthlyTotalPrice : null,
            MonthlyPaymentAmount = product.IsMonthlyInstallmentAvailable ? product.MonthlyPaymentAmount : null,
            MonthlyDownPayment = product.IsMonthlyInstallmentAvailable ? product.MonthlyDownPayment : null,
            RafidainTotalPrice = product.IsRafidainInstallmentAvailable ? product.RafidainTotalPrice : null,
            RafidainPaymentAmount = product.IsRafidainInstallmentAvailable ? product.RafidainPaymentAmount : null,
            RafidainDownPayment = product.IsRafidainInstallmentAvailable ? product.RafidainDownPayment : null,
            DailyTotalPrice = product.IsDailyInstallmentAvailable ? product.DailyTotalPrice : null,
            DailyPaymentAmount = product.IsDailyInstallmentAvailable ? product.DailyPaymentAmount : null,
            Images = product.Images
                .OrderBy(i => i.DisplayOrder)
                .Select(i => new ProductImagePublicDto { Id = i.Id, ImageUrl = i.ImageUrl, DisplayOrder = i.DisplayOrder })
                .ToList(),
            Category = new CategoryDetailDto
            {
                Id = product.Category.Id,
                Name = product.Category.Name,
                Slug = product.Category.Slug,
                AllowsCash = product.Category.AllowsCash,
                AllowsMonthlyInstallment = product.Category.AllowsMonthlyInstallment,
                AllowsDailyInstallment = product.Category.AllowsDailyInstallment,
                RequiresShopOwner = product.Category.RequiresShopOwner,
                MinInvoiceCash = product.Category.MinInvoiceCash,
                MinInvoiceInstallment = product.Category.MinInvoiceInstallment,
                HasCustomProductField = product.Category.HasCustomProductField,
            },
        };

        return Ok(dto);
    }
}
