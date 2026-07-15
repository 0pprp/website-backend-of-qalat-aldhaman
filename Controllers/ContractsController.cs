using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QalatAldhaman.Store.Api.Data;
using QalatAldhaman.Store.Api.DTOs.Public;

namespace QalatAldhaman.Store.Api.Controllers;

/// <summary>روابط ملفات عقود طرق الدفع المتوفرة فعلياً — للفرونت اند العام (بدون توثيق Swagger).</summary>
[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
[Route("api/contracts")]
public class ContractsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ContractsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<ContractLinkDto>>> GetAll()
    {
        var contracts = await _context.PurchaseMethodContracts
            .Where(c => c.ContractPdfUrl != null)
            .OrderBy(c => c.PurchaseMethod)
            .Select(c => new ContractLinkDto { PurchaseMethod = c.PurchaseMethod, ContractPdfUrl = c.ContractPdfUrl! })
            .ToListAsync();

        return Ok(contracts);
    }
}
