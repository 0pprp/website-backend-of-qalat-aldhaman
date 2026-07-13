# QalatAldhaman.Store.Api

باك اند مشروع متجر قلعة الضمان بالأقساط. مشروع **ASP.NET Core Web API (.NET 8)** منفصل تماماً عن أي مشروع أو قاعدة بيانات سابقة، يخدم موقع المتجر الإلكتروني (الفرونت اند: `qalat-aldhaman-gateway-main`).

**الباك اند جاهز بالكامل من ناحية الوظائف الأساسية**: السكيمة الكاملة (Categories, Products, ProductImages, Governorates, Orders, Reviews, AdminUsers)، توثيق الأدمن (JWT)، إدارة الفئات/المنتجات (CRUD + رفع ملفات + قراءة عامة)، استقبال طلبات الشراء بكل منطق التحقق الديناميكي وإدارتها، نظام آراء بموافقة إدارية، ولوحة إحصائيات (Dashboard) لصفحة تحكم الأدمن الرئيسية.

## المتطلبات

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- PostgreSQL (نسخة 14 أو أحدث) يعمل محلياً أو عبر Docker

## هيكلة المشروع

```
Controllers/           # نقاط دخول عامة (Health, Governorates, Uploads)
Controllers/Admin/     # نقاط دخول لوحة التحكم (Auth, Categories, Products) — كلها [Authorize] عدا Setup/Login
Controllers/Public/    # نقاط دخول عامة لعرض المتجر (Categories, Products)
Entities/              # كائنات النطاق (Domain entities)
Entities/Enums/        # PurchaseMethod, MediaType, OrderStatus
Data/                  # AppDbContext وإعدادات EF Core (العلاقات، الفهارس، الـ Seed)
DTOs/                  # كائنات نقل البيانات بين API والعميل
DTOs/Admin/            # DTOs كاملة الحقول (توثيق الأدمن + إدارة الفئات/المنتجات)
DTOs/Public/           # DTOs محدودة الحقول (بدون معلومات إدارية حساسة)
DTOs/Uploads/          # DTO الرد الخاص برفع الملفات
Services/              # JwtTokenService, SlugGenerator
Migrations/            # migrations الخاصة بـ EF Core
wwwroot/uploads/       # الملفات المرفوعة فعلياً (categories/, products/, contracts/) — مستثناة من Git
```

## قرارات تصميم السكيمة

- **المفتاح الأساسي**: `int` تلقائي التزايد (Identity) لكل الجداول بلا استثناء — أبسط للتطوير والتصحيح (debugging) والفرز الزمني الطبيعي، ولا حاجة لخصائص Guid (توزيع/أمان انكشاف) في هذا السياق (نظام داخلي خلف مصادقة).
- **تسمية الجداول/الأعمدة**: `snake_case` (مثل `product_images`, `min_invoice_cash`) عبر مكتبة [`EFCore.NamingConventions`](https://github.com/efcore/EFCore.NamingConventions) بدل الـ PascalCase الافتراضي من EF Core. السبب: PostgreSQL يُصغّر أي معرّف غير محاط بعلامات اقتباس تلقائياً، فاستخدام PascalCase يفرض اقتباس كل استعلام SQL يُكتب يدوياً (مثلاً بـ `psql` أو أي أداة إدارة)؛ `snake_case` هو العرف المتّبع بمجتمع PostgreSQL ويجعل الاستعلام المباشر أسهل بدون اقتباس.
- **تخزين الـ Enums**: كنص (`varchar`) وليس رقم صحيح، لتبقى قابلة للقراءة عند فحص الجدول مباشرة بقاعدة البيانات (مثلاً `status = 'Pending'` بدل `status = 0`).
- **حذف العلاقات (Delete Behavior)**: `Restrict` على علاقات `Order` (Product/Category/Governorate) للحفاظ على سجل الطلبات حتى لو حُذف المنتج/الفئة لاحقاً، و`Restrict` أيضاً بين `Product`→`Category`. أما `ProductImage` و`Review` فتُحذف تلقائياً (`Cascade`) عند حذف المنتج التابعة له لأنها بيانات فرعية لا قيمة لها بمفردها.
- **دقة الأعمدة العشرية**: كل حقول الأسعار `numeric(12,2)`، وحقول GPS (`GpsLat`/`GpsLng`) `numeric(9,6)`.
- **`Order.OrderNumber`**: يُولَّد عبر PostgreSQL sequence حقيقي (`order_number_seq`) وليس `MAX(Id)+1` أو عدّاد بجدول — راجع قسم "آلية توليد OrderNumber" أدناه.
- **Enums بصيغة نصية بالـ JSON أيضاً**: أضفت `JsonStringEnumConverter` عالمياً بـ `Program.cs` (`AddJsonOptions`)، فكل enum (`PurchaseMethod`, `MediaType`, `OrderStatus`) يُرسل ويُستقبل كنص قابل للقراءة (`"Cash"`, `"Pending"`...) بدل رقم صحيح — بكل الـ Requests/Responses وبتوثيق Swagger، وليس فقط بقاعدة البيانات.

## ضبط سلسلة الاتصال (Connection String)

سلسلة الاتصال في `appsettings.json` هي **placeholder فقط** ولا تحتوي بيانات حقيقية:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=REPLACE_ME;Port=5432;Database=REPLACE_ME;Username=REPLACE_ME;Password=REPLACE_ME"
}
```

لتشغيل المشروع محلياً، استبدلها عبر إحدى الطريقتين التاليتين (الاثنتان مستثناتان من Git):

### 1. `appsettings.Development.json` (الأسهل للتطوير المحلي)

الملف موجود بالمشروع ومُستثنى من Git عبر `.gitignore`. عدّل قيمة `ConnectionStrings:DefaultConnection` فيه لتطابق إعدادات PostgreSQL لديك، مثلاً:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=qalataldhaman_store;Username=postgres;Password=postgres"
}
```

### 2. User Secrets (بديل أكثر أماناً، خصوصاً لو المشروع مشترك بين عدة أجهزة)

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=qalataldhaman_store;Username=postgres;Password=postgres"
```

## إنشاء قاعدة البيانات محلياً

1. تأكد من تشغيل خدمة PostgreSQL على جهازك.
2. أنشئ قاعدة بيانات فارغة بالاسم الذي وضعته بسلسلة الاتصال (مثلاً `qalataldhaman_store`):

   ```bash
   psql -U postgres -c "CREATE DATABASE qalataldhaman_store;"
   ```

3. طبّق الـ Migrations (تُنشئ كل الجداول + تزرع المحافظات الـ12 تلقائياً):

   ```bash
   dotnet tool restore
   dotnet ef database update
   ```

   > المشروع يستخدم `dotnet-ef` كأداة محلية (Local Tool) مثبّتة بإصدار مطابق لـ EF Core 8 عبر `.config/dotnet-tools.json` — لهذا `dotnet tool restore` قبل أول استخدام يضمن استخدام نفس النسخة المتوافقة بدل أي نسخة عامة (Global) مختلفة قد تكون مثبتة على جهازك.

### إضافة migration جديدة لاحقاً (بعد أي تعديل بالـ Entities)

```bash
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

## التشغيل محلياً

```bash
dotnet restore
dotnet run
```

سيظهر رابط Swagger UI تلقائياً في الطرفية (منفذ HTTP الافتراضي حسب `Properties/launchSettings.json`)، عادة على شكل:

```
http://localhost:<port>/swagger
```

للتأكد من أن السيرفر يعمل، افتح `GET /api/health` — يجب أن يرجع `200 OK`.
للتأكد من أن السكيمة والـ Seed اشتغلوا صح، افتح `GET /api/governorates` — يجب أن يرجع 12 محافظة بالترتيب المطلوب.

## توثيق لوحة التحكم (JWT)

- تشفير كلمات المرور: `PasswordHasher<T>` من ASP.NET Core Identity (جزء من الـ shared framework، لا يحتاج حزمة NuGet إضافية) بدل BCrypt.Net — لأن المشروع لا يستخدم نظام Identity الكامل (لا يوجد UserManager/EF Identity Store، فقط جدول `AdminUser` مخصص بحقول محددة سلفاً بالسكيمة)، فاستخدام أداة التشفير وحدها (PBKDF2 بإصدارات قابلة للترقية مستقبلاً) هو الخيار الأخف والأنسب دون فرض بنية Identity الكاملة على السكيمة الحالية.
- إعدادات JWT بـ `appsettings.json` (placeholder فارغ، والمشروع **يرفض العمل عند التشغيل** إن كان `Jwt:SecretKey` فارغاً — فشل مبكر متعمّد بدل تشغيل غير آمن):

  ```json
  "Jwt": {
    "Issuer": "QalatAldhaman.Store.Api",
    "Audience": "QalatAldhaman.Store.Client",
    "ExpiryHours": 8,
    "SecretKey": ""
  },
  "AdminSetup": {
    "SetupKey": ""
  }
  ```

- القيم الفعلية (Secret Key عشوائي 64 بايت، و SetupKey عشوائي 32 بايت، مولّدة عبر `openssl rand -base64`) موضوعة فقط بـ `appsettings.Development.json` المحلي (مستثنى من Git). **لن تجد قيمهما بأي ملف بالمستودع** — سلّمت القيم مباشرة بالمحادثة وقت الإنشاء، وليس بأي ملف مُتتبَّع.

### نقاط الدخول

| Endpoint | الوصف |
|---|---|
| `POST /api/admin/auth/setup` | ينشئ **أول** حساب أدمن فقط (يتطلب `SetupKey` صحيح بالجسم). يرجع `403` إن كان يوجد أدمن مسبقاً بغض النظر عن صحة الـ SetupKey. |
| `POST /api/admin/auth/login` | `{ username, password }` → يرجع `{ token, username, fullName }`. رسالة خطأ عامة موحّدة عند فشل اليوزر أو الباسورد. |
| `GET /api/admin/auth/me` | محمي بـ `[Authorize]` — يرجع بيانات المستخدم المستخرجة من الـ JWT Claims فقط (بدون استعلام قاعدة بيانات). |

### إنشاء أول حساب أدمن فعلي

```bash
curl -X POST http://localhost:<port>/api/admin/auth/setup \
  -H "Content-Type: application/json" \
  -d '{"setupKey":"<القيمة المُسلَّمة بالمحادثة>","username":"...","password":"...","fullName":"..."}'
```

> ملاحظة: جرّبت هذا الـ Endpoint أثناء التطوير بحساب اختباري ثم حذفته من قاعدة البيانات مباشرة (وليس عبر أي Endpoint) لإعادة العدّاد لصفر — بإمكانك استخدام نفس الـ SetupKey لإنشاء الحساب الحقيقي الأول.

## رفع الملفات (Uploads)

`POST /api/uploads?folder={categories|products|contracts}` — **بدون توثيق** (متاح للزبائن أيضاً لاحقاً عند رفع صورة/فيديو الطلب، وليس فقط للأدمن). يستقبل multipart/form-data بحقل `file`، ويرجع `{ "url": "/uploads/<folder>/<guid>.<ext>" }`.

- التحقق: `folder=contracts` يقبل PDF فقط (حتى 20MB)، وأي `folder` آخر يقبل jpg/jpeg/png/webp فقط (حتى 10MB) — رفض أي شيء آخر برسالة `{ message }` واضحة و`400`.
- اسم الملف: `Guid.NewGuid()` + الامتداد الأصلي (لا تضارب أسماء أبداً).
- الملفات تُخزَّن فعلياً بـ `wwwroot/uploads/<folder>/` وتُقدَّم عبر `app.UseStaticFiles()` — الرابط المرجَّع يُفتح مباشرة (`http://<host>/uploads/...`).
- **قرار تقني مهم**: مجلدات `wwwroot/uploads/{categories,products,contracts}` تُنشأ تلقائياً بكود `Program.cs` **قبل** استدعاء `WebApplication.CreateBuilder`، وليس بعده — لأن ASP.NET Core يربط الـ static file provider بمسار `wwwroot` وقت بناء الـ Host، فلو المجلد غير موجود وقتها فلن يُخدَّم أي ملف يُنشأ لاحقاً أثناء التشغيل حتى لو كان الملف موجوداً فعلياً على القرص (اكتشفت هذا كخطأ فعلي أثناء الاختبار: أول رفع نجح والملف انحفظ، لكن رابطه رجع 404 لحين ما نقلت إنشاء المجلدات لقبل بناء الـ builder).

## إدارة الفئات (Admin) — محمي بـ `[Authorize]`

| Method | Path | الوصف |
|---|---|---|
| GET | `/api/admin/categories` | كل الفئات (فعّالة وغير فعّالة) |
| POST | `/api/admin/categories` | إنشاء فئة جديدة |
| PUT | `/api/admin/categories/{id}` | تعديل فئة |
| DELETE | `/api/admin/categories/{id}` | حذف نهائي إن لم يوجد منتجات مرتبطة، وإلا تعطيل (`IsActive = false`) بدل الحذف — الرد يوضّح أيهما حدث |

## إدارة المنتجات (Admin) — محمي بـ `[Authorize]`

| Method | Path | الوصف |
|---|---|---|
| GET | `/api/admin/products?categoryId=` | قائمة مع فلترة اختيارية |
| GET | `/api/admin/products/{id}` | تفاصيل منتج واحد |
| POST | `/api/admin/products` | إنشاء منتج — يتحقق من وجود `CategoryId` ومطابقة الأسعار المُرسلة لطرق الدفع المسموحة بالفئة |
| PUT | `/api/admin/products/{id}` | تعديل منتج (نفس التحقق) |
| DELETE | `/api/admin/products/{id}` | حذف نهائي إن لم توجد طلبات (`Order`) مرتبطة، وإلا تعطيل بدل الحذف |
| POST | `/api/admin/products/{id}/images` | إضافة صورة (رابط مرفوع مسبقاً عبر `/api/uploads`) |
| DELETE | `/api/admin/products/{id}/images/{imageId}` | حذف صورة |
| PUT | `/api/admin/products/{id}/contract` | تحديث رابط عقد PDF (مرفوع مسبقاً عبر `/api/uploads`) |

## Endpoints عامة (بدون توثيق) — لعرض المتجر

| Method | Path | الوصف |
|---|---|---|
| GET | `/api/categories` | الفئات الفعّالة فقط، حقول عرض فقط (بدون طرق الدفع/الحدود الإدارية) |
| GET | `/api/categories/{slug}/products` | منتجات فئة فعّالة (فعّالة فقط)، قائمة مختصرة (اسم/أسعار/صورة رئيسية) |
| GET | `/api/products/{id}` | تفاصيل منتج كامل: الصور، الأسعار الثلاثة، رابط العقد، **ومعلومات الفئة الكاملة** (طرق الدفع المسموحة، RequiresShopOwner، الحدود الدنيا، HasCustomProductField) لبناء فورم الطلب الديناميكي بالفرونت اند |
| POST | `/api/products/{productId}/reviews` | إرسال رأي (`CustomerName`, `Rating` 1-5, `Comment` اختياري) — يُحفظ دائماً بـ `IsApproved = false`، والرد رسالة "استُلم وسيظهر بعد المراجعة" فقط (ليس الرأي نفسه) |
| GET | `/api/products/{productId}/reviews` | الآراء **المعتمدة فقط**، الأحدث أولاً، بالإضافة إلى `AverageRating` و`TotalApprovedReviews` (يُحسبان من المعتمدة فقط) |

### قرار توليد الـ Slug

`Services/SlugGenerator.cs` **يبقي الأحرف العربية كما هي** بدل تحويلها صوتياً لحروف لاتينية (مثال: فئة "أجهزة كهربائية" ← `أجهزة-كهربائية`، وليس `ajhzة-khrbayiة` أو ما شابه). السبب: أي ترجمة صوتية عربي→لاتيني دقيقة تتطلب جدول تحويل معقد وغير موثوق (تتعدد الطرق الصحيحة لكتابة نفس الكلمة)، بينما روابط Unicode العربية مدعومة بالكامل بالمتصفحات الحديثة ومحركات البحث، ومستخدمة فعلياً بمواقع عربية كبرى (ويكيبيديا العربية مثلاً). عند التصادم يُضاف لاحقة `-2`, `-3`... حتى التفرد. إن أرسل الأدمن Slug صراحة يُمرَّر عبر نفس دالة التنظيف قبل التحقق من التفرد.

## الطلبات (Orders)

**قاعدة أساسية: لا يوجد سلة تسوق** — كل طلب = منتج واحد + طريقة دفع واحدة.

### `POST /api/orders` — بدون توثيق

يُنفَّذ التحقق **بالترتيب التالي بالضبط**، مع رسالة عربية محددة عند أول فشل (وليس رسالة عامة):

1. المنتج موجود وفعّال (`IsActive`) — وإلا `404`.
2. طريقة الدفع مسموحة بفئة المنتج (`AllowsCash`/`AllowsMonthlyInstallment`/`AllowsDailyInstallment`؛ `MonthlyRafidain` يتحقق من `AllowsMonthlyInstallment` أيضاً).
3. الحقول الإجبارية لطريقة الدفع المختارة (حسب الجدول أدناه) — رسالة الخطأ تذكر أسماء الحقول الناقصة بالضبط.
4. `RequiresShopOwner = true` ⇐ طريقة الدفع **يجب** أن تكون `DailyInstallment` (طبقة حماية إضافية فوق الشرط رقم 2).
5. `HasCustomProductField = true` ⇐ `CustomProductDescription` إجباري.
6. استخراج السعر من المنتج حسب طريقة الدفع؛ إن كان `null` ⇐ "هذا المنتج لا يتوفر بهذه الطريقة حالياً".
7. تحقق `MinInvoiceCash`/`MinInvoiceInstallment` — رسالة الخطأ تذكر المبلغ المطلوب بالضبط.
8. صيغة الهاتف العراقي (`^07\d{9}$`).
9. `GovernorateId` موجود فعلاً بجدول المحافظات.
10. إنشاء الطلب بحالة `Pending` و`PriceSnapshot` = السعر المستخرج بخطوة 6.

**الحقول الإجبارية حسب طريقة الدفع:**

| طريقة الدفع | الحقول الإجبارية (بالإضافة إلى CustomerName, PhoneNumber, GovernorateId دائماً) |
|---|---|
| `Cash` | (لا شيء إضافي) |
| `MonthlyInstallment` / `MonthlyRafidain` | `HomeAddress`, `NearestLandmark` |
| `DailyInstallment` | `ShopName`, `ShopAddress`, `NearestLandmark`, `MediaUrl`+`MediaType`, `GpsLat`+`GpsLng` |

**الرد عند النجاح:** `{ orderNumber, status, contractPdfUrl }` — `contractPdfUrl` هو ملف العقد **الثابت** الخاص بموديل المنتج نفسه (`Product.ContractPdfUrl`)، وليس مولَّداً لكل طلب.

### آلية توليد `OrderNumber` (بصيغة `ORD-00001`)

`Services/OrderNumberGenerator.cs` يستدعي `nextval('order_number_seq')` — وهو **PostgreSQL sequence حقيقي** (مُضاف عبر migration `AddOrderNumberSequence`)، وليس `SELECT MAX(Id)+1` ولا عدّاد بجدول عادي. السبب:

- `nextval()` **ذرّي (atomic)** على مستوى قاعدة البيانات نفسها — لا حاجة لـ transaction أو قفل صفوف يدوي بكودنا لضمان عدم التصادم.
- لا يحجز/يُقفل أي صف (بعكس عدّاد بجدول يحتاج `SELECT ... FOR UPDATE` فيبطئ الطلبات المتزامنة)، فطلبات كثيرة بنفس اللحظة تحصل كل واحدة على رقم مختلف فوراً بدون انتظار بعضها.
- **مُلاحظة مقصودة**: `nextval()` غير transactional عمداً بتصميم Postgres (لا يتراجع مع rollback)، فلو فشل إنشاء الطلب بعد توليد الرقم (نادر جداً) يحدث "فجوة" بالترقيم (مثلاً ORD-00005 قد لا يظهر أبداً). هذا سلوك مقبول ومتعارف عليه عالمياً لأرقام الفواتير/الطلبات — الأولوية للتفرد المضمون بدون تصادم، وليس للتسلسل بلا فجوات إطلاقاً.
- **اختبرته فعلياً**: أطلقت 8 طلبات إنشاء متزامنة بنفس اللحظة (`curl` بالخلفية بالتوازي)، ورجعت 8 أرقام مختلفة تماماً بدون أي تصادم.

### إدارة الطلبات (Admin) — محمي بـ `[Authorize]`

| Method | Path | الوصف |
|---|---|---|
| GET | `/api/admin/orders?status=&categoryId=&purchaseMethod=&governorateId=` | قائمة مع فلاتر اختيارية (الأحدث أولاً) |
| GET | `/api/admin/orders/{id}` | كل تفاصيل الطلب |
| PUT | `/api/admin/orders/{id}/status` | تحديث `Status` (`Pending → ContactedByRep → Confirmed/Rejected → Completed`) مع `Notes` اختياري |

## الآراء (Reviews) — إدارة (Admin) محمي بـ `[Authorize]`

| Method | Path | الوصف |
|---|---|---|
| GET | `/api/admin/reviews?pending=true&productId=` | `pending=true` يرجع غير المعتمدة فقط، وإلا الكل؛ فلترة اختيارية حسب المنتج |
| PUT | `/api/admin/reviews/{id}/approve` | يوافق على الرأي (يظهر بعدها بـ `GET /api/products/{productId}/reviews`) |
| DELETE | `/api/admin/reviews/{id}` | حذف نهائي (رفض/إزالة سبام) |

## لوحة الإحصائيات (Dashboard) — محمي بـ `[Authorize]`

`GET /api/admin/dashboard/stats` — يرجع كل أرقام الصفحة الرئيسية بلوحة التحكم بكائن واحد (`DashboardStatsDto`):

- `ordersByStatus`: عدد لكل حالة (Pending, ContactedByRep, Confirmed, Rejected, Completed) — تظهر كل الحالات حتى لو عددها 0.
- `totalOrders`, `ordersLast7Days`, `ordersLast30Days`.
- `ordersByCategory`: اسم الفئة + العدد، تنازلياً.
- `ordersByPurchaseMethod`: عدد لكل طريقة دفع (بما فيها `MonthlyRafidain`).
- `estimatedConfirmedRevenue`: **تقديري فقط** (مجموع `PriceSnapshot` لطلبات `Confirmed`/`Completed`) — **ليس** مبلغاً محصّلاً فعلياً، خصوصاً بطرق الأقساط حيث يُدفع على دفعات لاحقة.
- `recentOrders`: آخر 10 طلبات (ملخص مختصر).
- `pendingReviewsCount`, `activeProductsCount`, `activeCategoriesCount`.

**عدد الاستعلامات: 9 استعلامات بالضبط لكل استدعاء** (تأكدت من العدد فعلياً عبر سجل أوامر EF Core أثناء الاختبار)، كلها تجميع مباشر بمستوى قاعدة البيانات (`GROUP BY`/`COUNT`/`SUM`/`FILTER`) بدون أي حلقة استعلامات (N+1):

1. `GROUP BY status` (يُستخدم أيضاً لحساب `totalOrders` بالكود بجمع النتائج القليلة).
2. عدد آخر 7 و30 يوم بنفس الاستعلام الواحد (`COUNT(*) FILTER (WHERE ...)`).
3. `GROUP BY` اسم الفئة.
4. `GROUP BY` طريقة الدفع.
5. `SUM(price_snapshot)` للطلبات المؤكدة/المكتملة.
6. آخر 10 طلبات (`ORDER BY created_at DESC LIMIT 10`).
7. عدد الآراء غير المعتمدة.
8. عدد المنتجات الفعّالة.
9. عدد الفئات الفعّالة.

## CORS

مفعّل حالياً للسماح لفرونت اند التطوير على `http://localhost:8080` فقط (نفس منفذ مشروع `qalat-aldhaman-gateway-main` محلياً). أضف نطاق الإنتاج لاحقاً ضمن سياسة CORS في `Program.cs` عند النشر.

## ملاحظة مهمة

لا يوجد بأي ملف مرفوع لـ Git أي بيانات اتصال أو أسرار حقيقية. الملفات الحساسة (`appsettings.Development.json`, `secrets.json`, إلخ) مستثناة عبر `.gitignore`.
