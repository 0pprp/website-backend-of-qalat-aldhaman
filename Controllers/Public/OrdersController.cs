using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QalatAldhaman.Store.Api.Data;
using QalatAldhaman.Store.Api.DTOs.Public;
using QalatAldhaman.Store.Api.Entities;
using QalatAldhaman.Store.Api.Entities.Enums;
using QalatAldhaman.Store.Api.Services;

namespace QalatAldhaman.Store.Api.Controllers.Public;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private static readonly Regex IraqiPhoneRegex = new(@"^07\d{9}$", RegexOptions.Compiled);

    private readonly AppDbContext _context;
    private readonly OrderNumberGenerator _orderNumberGenerator;

    public OrdersController(AppDbContext context, OrderNumberGenerator orderNumberGenerator)
    {
        _context = context;
        _orderNumberGenerator = orderNumberGenerator;
    }

    [HttpPost]
    public async Task<ActionResult<CreateOrderResponseDto>> Create(CreateOrderRequestDto request)
    {
        // 1. المنتج موجود وفعّال
        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId);

        if (product is null || !product.IsActive)
        {
            return NotFound(new { message = "المنتج غير موجود أو غير متوفر حالياً" });
        }

        var category = product.Category;

        // 2. طريقة الدفع مسموحة بفئة هذا المنتج
        var methodAllowed = request.PurchaseMethod switch
        {
            PurchaseMethod.Cash => category.AllowsCash,
            PurchaseMethod.MonthlyInstallment or PurchaseMethod.MonthlyRafidain => category.AllowsMonthlyInstallment,
            PurchaseMethod.DailyInstallment => category.AllowsDailyInstallment,
            _ => false,
        };

        if (!methodAllowed)
        {
            return BadRequest(new { message = "طريقة الدفع هذه غير متاحة لفئة هذا المنتج" });
        }

        // 3. الحقول الإجبارية لطريقة الدفع المختارة
        var missingFields = GetMissingRequiredFields(request);
        if (missingFields.Count > 0)
        {
            return BadRequest(new { message = $"الحقول التالية مطلوبة: {string.Join("، ", missingFields)}" });
        }

        // 4. RequiresShopOwner => يجب أن تكون طريقة الدفع تقسيط يومي (حماية إضافية)
        if (category.RequiresShopOwner && request.PurchaseMethod != PurchaseMethod.DailyInstallment)
        {
            return BadRequest(new { message = "هذه الفئة تتطلب الشراء بالتقسيط اليومي فقط" });
        }

        // 5. HasCustomProductField => CustomProductDescription إجباري
        if (category.HasCustomProductField && string.IsNullOrWhiteSpace(request.CustomProductDescription))
        {
            return BadRequest(new { message = "الرجاء وصف المنتج المطلوب (CustomProductDescription)" });
        }

        // 6. استخراج السعر حسب طريقة الدفع
        decimal? price = request.PurchaseMethod switch
        {
            PurchaseMethod.Cash => product.CashPrice,
            PurchaseMethod.MonthlyInstallment or PurchaseMethod.MonthlyRafidain => product.MonthlyInstallmentPrice,
            PurchaseMethod.DailyInstallment => product.DailyInstallmentPrice,
            _ => null,
        };

        if (price is null)
        {
            return BadRequest(new { message = "هذا المنتج لا يتوفر بهذه الطريقة حالياً" });
        }

        // 7. الحد الأدنى للفاتورة
        var minRequired = request.PurchaseMethod == PurchaseMethod.Cash
            ? category.MinInvoiceCash
            : category.MinInvoiceInstallment;

        if (minRequired.HasValue && price.Value < minRequired.Value)
        {
            return BadRequest(new
            {
                message = $"الحد الأدنى لفاتورة هذه الفئة هو {minRequired.Value:N0} د.ع، وسعر هذا المنتج أقل من الحد المطلوب",
            });
        }

        // 8. صيغة رقم الهاتف
        if (!IraqiPhoneRegex.IsMatch(request.PhoneNumber))
        {
            return BadRequest(new { message = "رقم الهاتف غير صحيح، يجب أن يكون بصيغة 07XXXXXXXXX" });
        }

        // 9. المحافظة موجودة فعلاً
        var governorateExists = await _context.Governorates.AnyAsync(g => g.Id == request.GovernorateId);
        if (!governorateExists)
        {
            return BadRequest(new { message = "المحافظة المختارة غير موجودة" });
        }

        // 10. إنشاء الطلب
        var orderNumber = await _orderNumberGenerator.GenerateAsync();

        var order = new Order
        {
            OrderNumber = orderNumber,
            ProductId = product.Id,
            CategoryId = category.Id,
            PurchaseMethod = request.PurchaseMethod,
            CustomerName = request.CustomerName,
            PhoneNumber = request.PhoneNumber,
            GovernorateId = request.GovernorateId,
            ShopName = request.ShopName,
            ShopAddress = request.ShopAddress,
            HomeAddress = request.HomeAddress,
            NearestLandmark = request.NearestLandmark,
            MediaUrl = request.MediaUrl,
            MediaType = request.MediaType,
            GpsLat = request.GpsLat,
            GpsLng = request.GpsLng,
            CustomProductDescription = request.CustomProductDescription,
            PriceSnapshot = price.Value,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return Ok(new CreateOrderResponseDto
        {
            OrderNumber = order.OrderNumber,
            Status = order.Status,
            ContractPdfUrl = product.ContractPdfUrl,
        });
    }

    private static List<string> GetMissingRequiredFields(CreateOrderRequestDto request)
    {
        var missing = new List<string>();

        if (string.IsNullOrWhiteSpace(request.CustomerName)) missing.Add("الاسم الكامل");
        if (string.IsNullOrWhiteSpace(request.PhoneNumber)) missing.Add("رقم الهاتف");
        if (request.GovernorateId <= 0) missing.Add("المحافظة");

        switch (request.PurchaseMethod)
        {
            case PurchaseMethod.MonthlyInstallment:
            case PurchaseMethod.MonthlyRafidain:
                if (string.IsNullOrWhiteSpace(request.HomeAddress)) missing.Add("عنوان السكن");
                if (string.IsNullOrWhiteSpace(request.NearestLandmark)) missing.Add("أقرب نقطة دالة");
                break;

            case PurchaseMethod.DailyInstallment:
                if (string.IsNullOrWhiteSpace(request.ShopName)) missing.Add("اسم المحل");
                if (string.IsNullOrWhiteSpace(request.ShopAddress)) missing.Add("عنوان المحل");
                if (string.IsNullOrWhiteSpace(request.NearestLandmark)) missing.Add("أقرب نقطة دالة");
                if (string.IsNullOrWhiteSpace(request.MediaUrl) || request.MediaType is null) missing.Add("صورة أو فيديو المحل");
                if (request.GpsLat is null || request.GpsLng is null) missing.Add("إحداثيات الموقع (GPS)");
                break;
        }

        return missing;
    }
}
