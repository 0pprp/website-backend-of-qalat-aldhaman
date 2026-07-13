using Microsoft.EntityFrameworkCore;
using QalatAldhaman.Store.Api.Data;

namespace QalatAldhaman.Store.Api.Services;

/// <summary>
/// يولّد OrderNumber بصيغة ORD-00001 عبر PostgreSQL sequence (وليس MAX(Id)+1 أو عدّاد بجدول).
/// nextval() على sequence ذرّي (atomic) على مستوى قاعدة البيانات ولا يُحجز/يُقفل الصف مثل تحديث عدّاد
/// عادي، فطلبات متزامنة كثيرة تحصل كل واحدة على رقم مختلف بدون تصادم وبدون انتظار بعضها.
/// ملاحظة: nextval() غير transactional في Postgres عمداً (لا يتراجع مع rollback)، لذا قد تحدث
/// فجوات بالترقيم لو فشل إنشاء الطلب بعد توليد الرقم — هذا سلوك مقبول ومتعارف عليه لأرقام
/// الفواتير/الطلبات (الأولوية للتفرد المضمون وليس التسلسل بلا فجوات).
/// </summary>
public class OrderNumberGenerator
{
    private readonly AppDbContext _context;

    public OrderNumberGenerator(AppDbContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateAsync()
    {
        var next = await _context.Database
            .SqlQueryRaw<long>($"SELECT nextval('{AppDbContext.OrderNumberSequenceName}') AS \"Value\"")
            .FirstAsync();

        return $"ORD-{next:D5}";
    }
}
