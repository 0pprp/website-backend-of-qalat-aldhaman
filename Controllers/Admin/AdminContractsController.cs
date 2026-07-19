using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QalatAldhaman.Store.Api.Data;
using QalatAldhaman.Store.Api.DTOs.Admin;
using QalatAldhaman.Store.Api.Entities.Enums;

namespace QalatAldhaman.Store.Api.Controllers.Admin;

/// <summary>إدارة ملفات عقود/وصولات الأمانة — واحد لكل طريقة دفع، مشترك بين كل المنتجات.</summary>
[ApiController]
[Authorize]
[Route("api/admin/contracts")]
public class AdminContractsController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminContractsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<PurchaseMethodContractDto>>> GetAll()
    {
        var contracts = await _context.PurchaseMethodContracts
            .OrderBy(c => c.PurchaseMethod)
            .Select(c => new PurchaseMethodContractDto
            {
                PurchaseMethod = c.PurchaseMethod,
                ContractPdfUrl = c.ContractPdfUrl,
                UpdatedAt = c.UpdatedAt,
            })
            .ToListAsync();

        return Ok(contracts);
    }

    [HttpPut("{purchaseMethod}")]
    public async Task<ActionResult<PurchaseMethodContractDto>> Update(PurchaseMethod purchaseMethod, UpdateContractRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.ContractPdfUrl))
        {
            return BadRequest(new { message = "ContractPdfUrl مطلوب" });
        }

        var contract = await _context.PurchaseMethodContracts
            .FirstOrDefaultAsync(c => c.PurchaseMethod == purchaseMethod);

        if (contract is null)
        {
            return NotFound(new { message = "طريقة الدفع غير موجودة" });
        }

        contract.ContractPdfUrl = request.ContractPdfUrl;
        contract.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new PurchaseMethodContractDto
        {
            PurchaseMethod = contract.PurchaseMethod,
            ContractPdfUrl = contract.ContractPdfUrl,
            UpdatedAt = contract.UpdatedAt,
        });
    }

    /// <summary>
    /// يمسح رابط ملف العقد الحالي (يرجع الصف لحالة "لا يوجد ملف"). الصف نفسه (واحد ثابت لكل
    /// طريقة دفع، مزروع مسبقاً) لا يُحذف أبداً — فقط رابط الملف يُصفَّر إلى null.
    /// </summary>
    [HttpDelete("{purchaseMethod}")]
    public async Task<IActionResult> Clear(PurchaseMethod purchaseMethod)
    {
        var contract = await _context.PurchaseMethodContracts
            .FirstOrDefaultAsync(c => c.PurchaseMethod == purchaseMethod);

        if (contract is null)
        {
            return NotFound(new { message = "طريقة الدفع غير موجودة" });
        }

        contract.ContractPdfUrl = null;
        contract.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
