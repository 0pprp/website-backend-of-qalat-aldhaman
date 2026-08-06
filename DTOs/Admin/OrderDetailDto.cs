using QalatAldhaman.Store.Api.Entities.Enums;

namespace QalatAldhaman.Store.Api.DTOs.Admin;

public class OrderDetailDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>معبّأ لطلبات المنتج الواحد فقط — null لطلبات الباقات (راجع PackageId/Items).</summary>
    public int? ProductId { get; set; }
    public string? ProductName { get; set; }

    /// <summary>معبّأ لطلبات الباقات فقط.</summary>
    public int? PackageId { get; set; }
    public string? PackageName { get; set; }

    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public PurchaseMethod PurchaseMethod { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public int GovernorateId { get; set; }
    public string GovernorateName { get; set; } = string.Empty;
    public string? ShopName { get; set; }
    public string? ShopAddress { get; set; }
    public string? HomeAddress { get; set; }
    public string? NearestLandmark { get; set; }
    public string? MediaUrl { get; set; }
    public MediaType? MediaType { get; set; }
    public decimal? GpsLat { get; set; }
    public decimal? GpsLng { get; set; }
    public string? CustomProductDescription { get; set; }
    public decimal TotalPriceSnapshot { get; set; }
    public decimal? InstallmentPaymentAmountSnapshot { get; set; }
    public decimal? DownPaymentSnapshot { get; set; }
    public OrderStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ContractPdfUrl { get; set; }

    /// <summary>منتجات طلب الباقة — فارغة لطلبات المنتج الواحد العادية.</summary>
    public List<OrderItemDto> Items { get; set; } = [];
}

public class OrderItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPriceSnapshot { get; set; }
    public decimal? UnitPeriodicPaymentSnapshot { get; set; }
    public decimal? UnitDownPaymentSnapshot { get; set; }
    public int Quantity { get; set; }
}
