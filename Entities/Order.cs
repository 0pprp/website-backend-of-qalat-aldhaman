using QalatAldhaman.Store.Api.Entities.Enums;

namespace QalatAldhaman.Store.Api.Entities;

public class Order
{
    public int Id { get; set; }

    /// <summary>يُولَّد تلقائياً بصيغة ORD-00001</summary>
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>معبّأ لطلبات المنتج الواحد العادية، null لطلبات الباقات (راجع PackageId/OrderItems).</summary>
    public int? ProductId { get; set; }
    public Product? Product { get; set; }

    /// <summary>معبّأ لطلبات الباقات فقط — المنتجات المختارة تُسجَّل بـ OrderItems.</summary>
    public int? PackageId { get; set; }
    public Package? Package { get; set; }

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public PurchaseMethod PurchaseMethod { get; set; }

    public string CustomerName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;

    public int GovernorateId { get; set; }
    public Governorate Governorate { get; set; } = null!;

    public string? ShopName { get; set; }
    public string? ShopAddress { get; set; }
    public string? HomeAddress { get; set; }
    public string? NearestLandmark { get; set; }
    public string? MediaUrl { get; set; }
    public MediaType? MediaType { get; set; }
    public decimal? GpsLat { get; set; }
    public decimal? GpsLng { get; set; }
    public string? CustomProductDescription { get; set; }

    /// <summary>المبلغ الكلي وقت الطلب — يطابق CashPrice أو MonthlyTotalPrice أو DailyTotalPrice حسب طريقة الدفع.</summary>
    public decimal TotalPriceSnapshot { get; set; }

    /// <summary>الدفعة الدورية وقت الطلب (شهرية/يومية) — فارغة لطلبات النقد.</summary>
    public decimal? InstallmentPaymentAmountSnapshot { get; set; }

    /// <summary>مقدمة وقت الطلب — لطلبات الباقات فقط: مجموع مقدمات المنتجات المختارة (null إن لم يحدَّد أي منها مقدمة).</summary>
    public decimal? DownPaymentSnapshot { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<OrderItem> OrderItems { get; set; } = [];
}
