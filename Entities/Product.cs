using System.ComponentModel.DataAnnotations.Schema;

namespace QalatAldhaman.Store.Api.Entities;

public class Product
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    /// <summary>اسم الموديل</summary>
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? CashPrice { get; set; }

    /// <summary>المبلغ الكلي بالقسط الشهري</summary>
    public decimal? MonthlyTotalPrice { get; set; }

    /// <summary>الدفعة الشهرية</summary>
    public decimal? MonthlyPaymentAmount { get; set; }

    /// <summary>المقدمة بالقسط الشهري (اختيارية — لا تخضع لقاعدة "الحقلين معاً")</summary>
    public decimal? MonthlyDownPayment { get; set; }

    /// <summary>المبلغ الكلي بقسط الرافدين الشهري (للموظفين) — سعر منفصل عن القسط الشهري العادي</summary>
    public decimal? RafidainTotalPrice { get; set; }

    /// <summary>الدفعة الشهرية بقسط الرافدين</summary>
    public decimal? RafidainPaymentAmount { get; set; }

    /// <summary>المقدمة بقسط الرافدين (اختيارية — لا تخضع لقاعدة "الحقلين معاً")</summary>
    public decimal? RafidainDownPayment { get; set; }

    /// <summary>المبلغ الكلي بالقسط اليومي</summary>
    public decimal? DailyTotalPrice { get; set; }

    /// <summary>الدفعة اليومية</summary>
    public decimal? DailyPaymentAmount { get; set; }

    public string? SKU { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<ProductImage> Images { get; set; } = [];
    public List<Review> Reviews { get; set; } = [];

    /// <summary>
    /// طريقة القسط تُعتبر متوفرة فعلياً فقط إذا كان المبلغ الكلي والدفعة الدورية معبّيين معاً —
    /// تفادياً لعرض سعر ناقص للزبون إن نُسي أحد الحقلين.
    /// </summary>
    [NotMapped]
    public bool IsMonthlyInstallmentAvailable => MonthlyTotalPrice.HasValue && MonthlyPaymentAmount.HasValue;

    [NotMapped]
    public bool IsRafidainInstallmentAvailable => RafidainTotalPrice.HasValue && RafidainPaymentAmount.HasValue;

    [NotMapped]
    public bool IsDailyInstallmentAvailable => DailyTotalPrice.HasValue && DailyPaymentAmount.HasValue;
}
