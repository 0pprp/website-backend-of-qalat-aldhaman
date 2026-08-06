using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QalatAldhaman.Store.Api.Data;
using QalatAldhaman.Store.Api.DTOs.Admin;
using QalatAldhaman.Store.Api.Entities;
using QalatAldhaman.Store.Api.Entities.Enums;

namespace QalatAldhaman.Store.Api.Controllers.Admin;

[ApiController]
[Authorize]
[Route("api/admin/orders")]
public class AdminOrdersController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminOrdersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<OrderListItemDto>>> GetAll(
        [FromQuery] OrderStatus? status,
        [FromQuery] int? categoryId,
        [FromQuery] PurchaseMethod? purchaseMethod,
        [FromQuery] int? governorateId)
    {
        var query = _context.Orders
            .Include(o => o.Product)
            .Include(o => o.Package)
            .Include(o => o.Category)
            .Include(o => o.Governorate)
            .Include(o => o.OrderItems)
            .AsQueryable();

        if (status.HasValue) query = query.Where(o => o.Status == status.Value);
        if (categoryId.HasValue) query = query.Where(o => o.CategoryId == categoryId.Value);
        if (purchaseMethod.HasValue) query = query.Where(o => o.PurchaseMethod == purchaseMethod.Value);
        if (governorateId.HasValue) query = query.Where(o => o.GovernorateId == governorateId.Value);

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        var result = orders.Select(o => new OrderListItemDto
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            CustomerName = o.CustomerName,
            PhoneNumber = o.PhoneNumber,
            ProductName = o.Product?.Name ?? $"باقة: {o.Package?.Name} ({o.OrderItems.Count} منتجات)",
            CategoryName = o.Category.Name,
            GovernorateId = o.GovernorateId,
            GovernorateName = o.Governorate.Name,
            PurchaseMethod = o.PurchaseMethod,
            Status = o.Status,
            TotalPriceSnapshot = o.TotalPriceSnapshot,
            InstallmentPaymentAmountSnapshot = o.InstallmentPaymentAmountSnapshot,
            DownPaymentSnapshot = o.DownPaymentSnapshot,
            CreatedAt = o.CreatedAt,
        }).ToList();

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderDetailDto>> GetOne(int id)
    {
        var order = await _context.Orders
            .Include(o => o.Product)
            .Include(o => o.Package)
            .Include(o => o.Category)
            .Include(o => o.Governorate)
            .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null)
        {
            return NotFound(new { message = "الطلب غير موجود" });
        }

        return Ok(await ToDetailDtoAsync(order));
    }

    [HttpPut("{id:int}/status")]
    public async Task<ActionResult<OrderDetailDto>> UpdateStatus(int id, UpdateOrderStatusRequestDto request)
    {
        var order = await _context.Orders
            .Include(o => o.Product)
            .Include(o => o.Package)
            .Include(o => o.Category)
            .Include(o => o.Governorate)
            .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null)
        {
            return NotFound(new { message = "الطلب غير موجود" });
        }

        order.Status = request.Status;
        if (request.Notes is not null)
        {
            order.Notes = request.Notes;
        }

        await _context.SaveChangesAsync();

        return Ok(await ToDetailDtoAsync(order));
    }

    /// <summary>حذف طلب نهائي — لا شيء بقاعدة البيانات يرتبط بالطلب كأب (leaf entity)، فلا قيود Restrict تعترض هذا الحذف.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order is null)
        {
            return NotFound(new { message = "الطلب غير موجود" });
        }

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private async Task<OrderDetailDto> ToDetailDtoAsync(Order o)
    {
        var contractUrl = await _context.PurchaseMethodContracts
            .Where(c => c.PurchaseMethod == o.PurchaseMethod)
            .Select(c => c.ContractPdfUrl)
            .FirstOrDefaultAsync();

        return new OrderDetailDto
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            ProductId = o.ProductId,
            ProductName = o.Product?.Name,
            PackageId = o.PackageId,
            PackageName = o.Package?.Name,
            CategoryId = o.CategoryId,
            CategoryName = o.Category.Name,
            PurchaseMethod = o.PurchaseMethod,
            CustomerName = o.CustomerName,
            PhoneNumber = o.PhoneNumber,
            GovernorateId = o.GovernorateId,
            GovernorateName = o.Governorate.Name,
            ShopName = o.ShopName,
            ShopAddress = o.ShopAddress,
            HomeAddress = o.HomeAddress,
            NearestLandmark = o.NearestLandmark,
            MediaUrl = o.MediaUrl,
            MediaType = o.MediaType,
            GpsLat = o.GpsLat,
            GpsLng = o.GpsLng,
            CustomProductDescription = o.CustomProductDescription,
            TotalPriceSnapshot = o.TotalPriceSnapshot,
            InstallmentPaymentAmountSnapshot = o.InstallmentPaymentAmountSnapshot,
            DownPaymentSnapshot = o.DownPaymentSnapshot,
            Status = o.Status,
            Notes = o.Notes,
            CreatedAt = o.CreatedAt,
            ContractPdfUrl = contractUrl,
            Items = o.OrderItems.Select(oi => new OrderItemDto
            {
                ProductId = oi.ProductId,
                ProductName = oi.Product.Name,
                UnitPriceSnapshot = oi.UnitPriceSnapshot,
                UnitPeriodicPaymentSnapshot = oi.UnitPeriodicPaymentSnapshot,
                UnitDownPaymentSnapshot = oi.UnitDownPaymentSnapshot,
            }).ToList(),
        };
    }
}
