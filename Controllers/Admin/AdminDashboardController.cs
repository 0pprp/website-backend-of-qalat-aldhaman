using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QalatAldhaman.Store.Api.Data;
using QalatAldhaman.Store.Api.DTOs.Admin;
using QalatAldhaman.Store.Api.Entities.Enums;

namespace QalatAldhaman.Store.Api.Controllers.Admin;

[ApiController]
[Authorize]
[Route("api/admin/dashboard")]
public class AdminDashboardController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminDashboardController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("stats")]
    public async Task<ActionResult<DashboardStatsDto>> GetStats()
    {
        var now = DateTime.UtcNow;
        var sevenDaysAgo = now.AddDays(-7);
        var thirtyDaysAgo = now.AddDays(-30);

        // 1. Orders grouped by status (also yields the total).
        var statusCounts = await _context.Orders
            .GroupBy(o => o.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var ordersByStatus = Enum.GetValues<OrderStatus>()
            .ToDictionary(s => s.ToString(), s => statusCounts.FirstOrDefault(x => x.Status == s)?.Count ?? 0);

        var totalOrders = statusCounts.Sum(x => x.Count);

        // 2. Last 7 / 30 days counts computed in a single query.
        var recentCounts = await _context.Orders
            .Where(o => o.CreatedAt >= thirtyDaysAgo)
            .GroupBy(o => 1)
            .Select(g => new
            {
                Last30 = g.Count(),
                Last7 = g.Count(o => o.CreatedAt >= sevenDaysAgo),
            })
            .FirstOrDefaultAsync();

        // 3. Orders by category.
        var ordersByCategory = await _context.Orders
            .GroupBy(o => o.Category.Name)
            .Select(g => new CategoryOrderCountDto { CategoryName = g.Key, OrderCount = g.Count() })
            .OrderByDescending(x => x.OrderCount)
            .ToListAsync();

        // 4. Orders by purchase method.
        var methodCounts = await _context.Orders
            .GroupBy(o => o.PurchaseMethod)
            .Select(g => new { Method = g.Key, Count = g.Count() })
            .ToListAsync();

        var ordersByPurchaseMethod = Enum.GetValues<PurchaseMethod>()
            .ToDictionary(m => m.ToString(), m => methodCounts.FirstOrDefault(x => x.Method == m)?.Count ?? 0);

        // 5. Estimated confirmed revenue (Confirmed + Completed only).
        var estimatedConfirmedRevenue = await _context.Orders
            .Where(o => o.Status == OrderStatus.Confirmed || o.Status == OrderStatus.Completed)
            .SumAsync(o => o.PriceSnapshot);

        // 6. Last 10 orders.
        var recentOrders = await _context.Orders
            .OrderByDescending(o => o.CreatedAt)
            .Take(10)
            .Select(o => new RecentOrderDto
            {
                OrderNumber = o.OrderNumber,
                CustomerName = o.CustomerName,
                ProductName = o.Product.Name,
                PurchaseMethod = o.PurchaseMethod,
                Status = o.Status,
                CreatedAt = o.CreatedAt,
            })
            .ToListAsync();

        // 7-9. Pending reviews / active products / active categories.
        var pendingReviewsCount = await _context.Reviews.CountAsync(r => !r.IsApproved);
        var activeProductsCount = await _context.Products.CountAsync(p => p.IsActive);
        var activeCategoriesCount = await _context.Categories.CountAsync(c => c.IsActive);

        return Ok(new DashboardStatsDto
        {
            OrdersByStatus = ordersByStatus,
            TotalOrders = totalOrders,
            OrdersLast7Days = recentCounts?.Last7 ?? 0,
            OrdersLast30Days = recentCounts?.Last30 ?? 0,
            OrdersByCategory = ordersByCategory,
            OrdersByPurchaseMethod = ordersByPurchaseMethod,
            EstimatedConfirmedRevenue = estimatedConfirmedRevenue,
            RecentOrders = recentOrders,
            PendingReviewsCount = pendingReviewsCount,
            ActiveProductsCount = activeProductsCount,
            ActiveCategoriesCount = activeCategoriesCount,
        });
    }
}
