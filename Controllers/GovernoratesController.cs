using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QalatAldhaman.Store.Api.Data;
using QalatAldhaman.Store.Api.DTOs;

namespace QalatAldhaman.Store.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GovernoratesController : ControllerBase
{
    private readonly AppDbContext _context;

    public GovernoratesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<GovernorateDto>>> GetAll()
    {
        var governorates = await _context.Governorates
            .OrderBy(g => g.Id)
            .Select(g => new GovernorateDto { Id = g.Id, Name = g.Name })
            .ToListAsync();

        return Ok(governorates);
    }
}
