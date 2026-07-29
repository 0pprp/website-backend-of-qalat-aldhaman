namespace QalatAldhaman.Store.Api.Entities;

/// <summary>باقة شراء لفئة تستخدم نظام الباقات (Category.UsesPackages) — الزبون يختار منتجات من نفس الفئة حتى يبلغ مجموع أسعارها هذا الحد.</summary>
public class Package
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public decimal MinimumTotalPrice { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}
