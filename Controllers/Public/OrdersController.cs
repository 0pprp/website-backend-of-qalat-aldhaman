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
            .Include(o => o.Package)
            .ToListAsync();

        var contractUrlsByMethod = await _context.PurchaseMethodContracts
            .ToDictionaryAsync(c => c.PurchaseMethod, c => c.ContractPdfUrl);

        var result = orders.Select(o => new OrderLookupResultDto
        {
            OrderNumber = o.OrderNumber,
            ProductName = o.Product?.Name ?? $"باقة: {o.Package?.Name}",
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

        // 1.5. الفئات التي تُشترى فقط عبر باقة لا تقبل طلب منتج واحد مباشر
        if (category.UsesPackages)
        {
            return BadRequest(new { message = "هذه الفئة تُشترى فقط عبر اختيار باقة، استخدم /api/orders/package" });
        }

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
        var missingFields = GetMissingRequiredFields(
            request.PurchaseMethod, request.CustomerName, request.PhoneNumber, request.GovernorateId,
            request.HomeAddress, request.NearestLandmark, request.ShopName, request.ShopAddress,
            request.MediaUrl, request.MediaType, request.GpsLat, request.GpsLng);

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
        var (totalPrice, paymentAmount, _) = ResolveProductPricing(product, request.PurchaseMethod);

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

    [HttpPost("package")]
    public async Task<ActionResult<CreateOrderResponseDto>> CreatePackageOrder(CreatePackageOrderRequestDto request)
    {
        // 1. الباقة موجودة وفعّالة، وتتبع فئة تستخدم الباقات
        var package = await _context.Packages
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == request.PackageId);

        if (package is null || !package.IsActive)
        {
            return NotFound(new { message = "الباقة غير موجودة أو غير متوفرة حالياً" });
        }

        var category = package.Category;

        if (!category.UsesPackages)
        {
            return BadRequest(new { message = "فئة هذه الباقة لا تستخدم نظام الباقات" });
        }

        // 2. طريقة الدفع مسموحة بالفئة
        var methodAllowed = request.PurchaseMethod switch
        {
            PurchaseMethod.Cash => category.AllowsCash,
            PurchaseMethod.MonthlyInstallment or PurchaseMethod.MonthlyRafidain => category.AllowsMonthlyInstallment,
            PurchaseMethod.DailyInstallment => category.AllowsDailyInstallment,
            _ => false,
        };

        if (!methodAllowed)
        {
            return BadRequest(new { message = "طريقة الدفع هذه غير متاحة لهذه الفئة" });
        }

        // 3. المنتجات المختارة: موجودة، تتبع نفس فئة الباقة، فعّالة، ولها سعر صالح لطريقة الدفع المختارة
        if (request.ProductIds is null || request.ProductIds.Count == 0)
        {
            return BadRequest(new { message = "يجب اختيار منتج واحد على الأقل ضمن الباقة" });
        }

        var distinctProductIds = request.ProductIds.Distinct().ToList();
        var products = await _context.Products
            .Where(p => distinctProductIds.Contains(p.Id))
            .ToListAsync();

        var missingIds = distinctProductIds.Except(products.Select(p => p.Id)).ToList();
        if (missingIds.Count > 0)
        {
            return BadRequest(new { message = $"منتج غير موجود (معرّف: {string.Join(", ", missingIds)})" });
        }

        var pricedProducts = new List<(Product Product, decimal Total, decimal? Payment, decimal? DownPayment)>();
        foreach (var product in products)
        {
            if (product.CategoryId != category.Id)
            {
                return BadRequest(new { message = $"المنتج \"{product.Name}\" لا ينتمي لفئة هذه الباقة" });
            }

            if (!product.IsActive)
            {
                return BadRequest(new { message = $"المنتج \"{product.Name}\" غير متوفر حالياً" });
            }

            var (total, payment, downPayment) = ResolveProductPricing(product, request.PurchaseMethod);
            if (total is null)
            {
                return BadRequest(new { message = $"المنتج \"{product.Name}\" لا يتوفر بطريقة الدفع المختارة" });
            }

            pricedProducts.Add((product, total.Value, payment, downPayment));
        }

        // 4. المجموع يجب أن يبلغ حد الباقة الأدنى على الأقل
        var totalSum = pricedProducts.Sum(p => p.Total);
        if (totalSum < package.MinimumTotalPrice)
        {
            return BadRequest(new
            {
                message = $"مجموع أسعار المنتجات المختارة ({totalSum:N0} د.ع) أقل من الحد الأدنى المطلوب لهذه الباقة ({package.MinimumTotalPrice:N0} د.ع)",
            });
        }

        // 5. الحقول الإجبارية لطريقة الدفع المختارة (نفس القواعد المعتادة)
        var missingFields = GetMissingRequiredFields(
            request.PurchaseMethod, request.CustomerName, request.PhoneNumber, request.GovernorateId,
            request.HomeAddress, request.NearestLandmark, request.ShopName, request.ShopAddress,
            request.MediaUrl, request.MediaType, request.GpsLat, request.GpsLng);

        if (missingFields.Count > 0)
        {
            return BadRequest(new { message = $"الحقول التالية مطلوبة: {string.Join("، ", missingFields)}" });
        }

        if (category.RequiresShopOwner && request.PurchaseMethod != PurchaseMethod.DailyInstallment)
        {
            return BadRequest(new { message = "هذه الفئة تتطلب الشراء بالتقسيط اليومي فقط" });
        }

        if (category.HasCustomProductField && string.IsNullOrWhiteSpace(request.CustomProductDescription))
        {
            return BadRequest(new { message = "الرجاء وصف المنتج المطلوب (CustomProductDescription)" });
        }

        if (!IraqiPhoneRegex.IsMatch(request.PhoneNumber))
        {
            return BadRequest(new { message = "رقم الهاتف غير صحيح، يجب أن يكون بصيغة 07XXXXXXXXX" });
        }

        var governorateExists = await _context.Governorates.AnyAsync(g => g.Id == request.GovernorateId);
        if (!governorateExists)
        {
            return BadRequest(new { message = "المحافظة المختارة غير موجودة" });
        }

        // 6. إنشاء الطلب + عناصره
        var orderNumber = await _orderNumberGenerator.GenerateAsync();

        var installmentSum = request.PurchaseMethod == PurchaseMethod.Cash
            ? (decimal?)null
            : pricedProducts.Sum(p => p.Payment ?? 0);

        var downPaymentSum = pricedProducts.All(p => p.DownPayment is null)
            ? (decimal?)null
            : pricedProducts.Sum(p => p.DownPayment ?? 0);

        var order = new Order
        {
            OrderNumber = orderNumber,
            ProductId = null,
            PackageId = package.Id,
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
            TotalPriceSnapshot = totalSum,
            InstallmentPaymentAmountSnapshot = installmentSum,
            DownPaymentSnapshot = downPaymentSum,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            OrderItems = pricedProducts.Select(p => new OrderItem
            {
                ProductId = p.Product.Id,
                UnitPriceSnapshot = p.Total,
                UnitPeriodicPaymentSnapshot = p.Payment,
                UnitDownPaymentSnapshot = p.DownPayment,
            }).ToList(),
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

    /// <summary>يستخرج (المبلغ الكلي، الدفعة الدورية، المقدمة) لمنتج معيّن حسب طريقة دفع مختارة — null بالمبلغ الكلي يعني الطريقة غير متوفرة لهذا المنتج.</summary>
    private static (decimal? Total, decimal? Payment, decimal? DownPayment) ResolveProductPricing(Product product, PurchaseMethod method) => method switch
    {
        PurchaseMethod.Cash => (product.CashPrice, null, null),
        PurchaseMethod.MonthlyInstallment => product.IsMonthlyInstallmentAvailable
            ? (product.MonthlyTotalPrice, product.MonthlyPaymentAmount, product.MonthlyDownPayment)
            : (null, null, null),
        PurchaseMethod.MonthlyRafidain => product.IsRafidainInstallmentAvailable
            ? (product.RafidainTotalPrice, product.RafidainPaymentAmount, product.RafidainDownPayment)
            : (null, null, null),
        PurchaseMethod.DailyInstallment => product.IsDailyInstallmentAvailable
            ? (product.DailyTotalPrice, product.DailyPaymentAmount, null)
            : (null, null, null),
        _ => (null, null, null),
    };

    private static List<string> GetMissingRequiredFields(
        PurchaseMethod purchaseMethod, string customerName, string phoneNumber, int governorateId,
        string? homeAddress, string? nearestLandmark, string? shopName, string? shopAddress,
        string? mediaUrl, MediaType? mediaType, decimal? gpsLat, decimal? gpsLng)
    {
        var missing = new List<string>();

        if (string.IsNullOrWhiteSpace(customerName)) missing.Add("الاسم الكامل");
        if (string.IsNullOrWhiteSpace(phoneNumber)) missing.Add("رقم الهاتف");
        if (governorateId <= 0) missing.Add("المحافظة");

        switch (purchaseMethod)
        {
            case PurchaseMethod.MonthlyInstallment:
            case PurchaseMethod.MonthlyRafidain:
                if (string.IsNullOrWhiteSpace(homeAddress)) missing.Add("عنوان السكن");
                if (string.IsNullOrWhiteSpace(nearestLandmark)) missing.Add("أقرب نقطة دالة");
                break;

            case PurchaseMethod.DailyInstallment:
                if (string.IsNullOrWhiteSpace(shopName)) missing.Add("اسم المحل");
                if (string.IsNullOrWhiteSpace(shopAddress)) missing.Add("عنوان المحل");
                if (string.IsNullOrWhiteSpace(nearestLandmark)) missing.Add("أقرب نقطة دالة");
                if (string.IsNullOrWhiteSpace(mediaUrl) || mediaType is null) missing.Add("صورة أو فيديو المحل");
                if (gpsLat is null || gpsLng is null) missing.Add("إحداثيات الموقع (GPS)");
                break;
        }

        return missing;
    }
}
