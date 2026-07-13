namespace QalatAldhaman.Store.Api.DTOs.Admin;

public class DashboardStatsDto
{
    public Dictionary<string, int> OrdersByStatus { get; set; } = new();
    public int TotalOrders { get; set; }
    public int OrdersLast7Days { get; set; }
    public int OrdersLast30Days { get; set; }
    public List<CategoryOrderCountDto> OrdersByCategory { get; set; } = [];
    public Dictionary<string, int> OrdersByPurchaseMethod { get; set; } = new();

    /// <summary>
    /// تقديري فقط (مجموع PriceSnapshot لطلبات Confirmed/Completed) — ليس مبلغاً محصّلاً فعلياً،
    /// خصوصاً بطرق الأقساط حيث يُدفع على دفعات لاحقة وليس دفعة واحدة عند التأكيد.
    /// </summary>
    public decimal EstimatedConfirmedRevenue { get; set; }

    public List<RecentOrderDto> RecentOrders { get; set; } = [];
    public int PendingReviewsCount { get; set; }
    public int ActiveProductsCount { get; set; }
    public int ActiveCategoriesCount { get; set; }
}
