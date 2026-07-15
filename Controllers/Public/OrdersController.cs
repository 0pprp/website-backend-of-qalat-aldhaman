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

    [HttpGet("lookup")]
    public async Task<ActionResult<List<OrderLookupResultDto>>> Lookup([FromQuery] string phone)
    {
        if (string.IsNullOrWhiteSpace(phone) || !IraqiPhoneRegex.IsMatch(phone))
        {
            return BadRequest(new { message = "رقم الهاتف غير صحيح، يجب أن يكون بصيغة 07XXXXXXXXX" });
        }

        var orders = await _context.Orders
            .Where(o => o.PhoneNumber == phone)
            .OrderByDescending(o => o.CreatedAt)
            .Include(o => o.Product)
            .ToListAsync();

        var contractUrlsByMethod = await _context.PurchaseMethodContracts
            .ToDictionaryAsync(c => c.PurchaseMethod, c => c.ContractPdfUrl);

        var result = orders.Select(o => new OrderLookupResultDto
        {
            OrderNumber = o.OrderNumber,
            ProductName = o.Product.Name,
            PurchaseMethod = o.PurchaseMethod,
            TotalPriceSnapshot = o.TotalPriceSnapshot,
            InstallmentPaymentAmountSnapshot = o.InstallmentPaymentAmountSnapshot,
            Status = o.Status,
            CreatedAt = o.CreatedAt,
            ContractPdfUrl = contractUrlsByMethod.GetValueOrDefault(o.PurchaseMethod),
        }).ToList();

        return Ok(result);
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

        // 6. استخراج السعر حسب طريقة الدفع — للقسط: المبلغ الكلي + الدفعة الدورية (يجب أن يكونا معبّيين معاً،
        // وإلا تُعامل الطريقة كغير متوفرة أصلاً، بنفس منطق IsMonthlyInstallmentAvailable/IsDailyInstallmentAvailable).
        decimal? totalPrice;
        decimal? paymentAmount;

        switch (request.PurchaseMethod)
        {
            case PurchaseMethod.Cash:
                totalPrice = product.CashPrice;
                paymentAmount = null;
                break;
            case PurchaseMethod.MonthlyInstallment:
            case PurchaseMethod.MonthlyRafidain:
                totalPrice = product.IsMonthlyInstallmentAvailable ? product.MonthlyTotalPrice : null;
                paymentAmount = product.IsMonthlyInstallmentAvailable ? product.MonthlyPaymentAmount : null;
                break;
            case PurchaseMethod.DailyInstallment:
                totalPrice = product.IsDailyInstallmentAvailable ? product.DailyTotalPrice : null;
                paymentAmount = product.IsDailyInstallmentAvailable ? product.DailyPaymentAmount : null;
                break;
            default:
                totalPrice = null;
                paymentAmount = null;
                break;
        }

        if (totalPrice is null)
        {
            return BadRequest(new { message = "هذا المنتج لا يتوفر بهذه الطريقة حالياً" });
        }

        // 7. الحد الأدنى للفاتورة (مقارنة بالمبلغ الكلي، وليس الدفعة الدورية)
        var minRequired = request.PurchaseMethod == PurchaseMethod.Cash
            ? category.MinInvoiceCash
            : category.MinInvoiceInstallment;

        if (minRequired.HasValue && totalPrice.Value < minRequired.Value)
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
            TotalPriceSnapshot = totalPrice.Value,
            InstallmentPaymentAmountSnapshot = paymentAmount,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        var contractUrl = await _context.PurchaseMethodContracts
            .Where(c => c.PurchaseMethod == order.PurchaseMethod)
            .Select(c => c.ContractPdfUrl)
            .FirstOrDefaultAsync();

        return Ok(new CreateOrderResponseDto
        {
            OrderNumber = order.OrderNumber,
            Status = order.Status,
            ContractPdfUrl = contractUrl,
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
