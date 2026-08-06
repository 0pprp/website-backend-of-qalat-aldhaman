using System.ComponentModel.DataAnnotations.Schema;

namespace QalatAldhaman.Store.Api.Entities;

/// <summary>باقة شراء لفئة تستخدم نظام الباقات (Category.UsesPackages) — الزبون يختار منتجات من نفس الفئة حتى يبلغ مجموع أسعارها هذا الحد.</summary>
public class Package
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    /// <summary>الحد الأدنى لمجموع أسعار المنتجات المختارة داخل الباقة — لا يُستخدم كسعر الطلب النهائي.</summary>
    public decimal MinimumTotalPrice { get; set; }

    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }

    /// <summary>سعر الباقة الخاص بها — مستقل عن أسعار المنتجات بداخلها، ونفس بنية أسعار Product بالضبط.</summary>
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

    /// <summary>
    /// طريقة القسط تُعتبر متوفرة فعلياً فقط إذا كان المبلغ الكلي والدفعة الدورية معبّيين معاً —
    /// نفس منطق Product بالضبط.
    /// </summary>
    [NotMapped]
    public bool IsMonthlyInstallmentAvailable => MonthlyTotalPrice.HasValue && MonthlyPaymentAmount.HasValue;

    [NotMapped]
    public bool IsRafidainInstallmentAvailable => RafidainTotalPrice.HasValue && RafidainPaymentAmount.HasValue;

    [NotMapped]
    public bool IsDailyInstallmentAvailable => DailyTotalPrice.HasValue && DailyPaymentAmount.HasValue;
}
