namespace QalatAldhaman.Store.Api.Entities;

/// <summary>سطر منتج واحد ضمن طلب باقة (Order.PackageId معبّأ) — لا يُستخدم لطلبات المنتج الواحد العادية.</summary>
public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    /// <summary>سعر هذا المنتج وقت الطلب، حسب طريقة الدفع المختارة لكامل طلب الباقة.</summary>
    public decimal UnitPriceSnapshot { get; set; }
    public decimal? UnitPeriodicPaymentSnapshot { get; set; }
    public decimal? UnitDownPaymentSnapshot { get; set; }
}
