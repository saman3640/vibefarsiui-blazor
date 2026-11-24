# وایب‌فارسی — VibeFarsiUI for .NET 10 Blazor

کامپوننت‌های فارسی، راست‌چین و RTL-first برای **ASP.NET Core Blazor Web App** روی **.NET 10**.
این مخزن مهاجرت سایت و اکوسیستم [vibefarsiui](https://github.com/TronIsHere/vibefarsiui) از Next.js/React به Blazor است؛ رجیستری اصلی، تم‌ها، فونت‌ها، محتوا و قراردادهای ماشینی حفظ شده‌اند.

> **وضعیت مهاجرت (صادقانه):** زیرساخت، رجیستری، تم‌ها، تقویم جلالی، قالب‌بندی فارسی و ۳۶ اسلاگ کامپوننت (۳۵ نوع Razor) با پیش‌نمایش زنده تکمیل شده‌اند. از ۱۰۸ کامپوننت رجیستری، ۷۲ مورد هنوز در صف مهاجرت‌اند و در سایت با وضعیت شفاف، متادیتا، سورس اصلی و پرامپت نمایش داده می‌شوند — نه با پیش‌نمایش جعلی.
> فهرست کامل: [`docs/COMPONENT_MIGRATION_STATUS.md`](docs/COMPONENT_MIGRATION_STATUS.md) · تحلیل: [`docs/MIGRATION_ANALYSIS.md`](docs/MIGRATION_ANALYSIS.md)

## اجرا

پیش‌نیاز: [.NET 10 SDK](https://dotnet.microsoft.com/download) (نسخه‌ی پین‌شده در `global.json`: `10.0.404`).

```bash
dotnet restore
dotnet build
dotnet test
dotnet run --project src/VibeFarsiUI.Web
```

سپس `http://localhost:5xxx` را باز کنید (پورت در خروجی مشخص می‌شود؛ برای پورت ثابت: `dotnet run --project src/VibeFarsiUI.Web --urls http://127.0.0.1:5188`).

### Visual Studio 2026

1. `VibeFarsiUI.sln` را باز کنید.
2. `VibeFarsiUI.Web` را به‌عنوان Startup Project تنظیم کنید.
3. F5 بزنید. رندر تعاملی Server است و به Node.js نیاز ندارد.

### Docker

```bash
docker build -t vibefarsiui-blazor .
docker run --rm -p 8080:8080 vibefarsiui-blazor
```

## معماری

```
VibeFarsiUI.sln
src/
  VibeFarsiUI.Web/            Blazor Web App، صفحات، کامپوننت‌های Razor، wwwroot
    Components/
      Layout/ Pages/ Shared/ UI/
    Catalog/                  نگاشت کامپوننت‌های بومی و جدول پارامترها
    Services/                 CodeHighlighter، RazorSourceCatalog
    wwwroot/
      css/                    themes.css توکن‌های اصلی · app.css · components.css
      js/site.js              تم، clipboard، فوکوس OTP
      fonts/                  IRANSans (وزن‌های ۳۰۰/۴۰۰/۶۰۰/۷۰۰)
      registry/               catalog.json و documents.json صادرشده از رجیستری اصلی
  VibeFarsiUI.Core/           تقویم جلالی، PersianText، اعتبارسنجی ایرانی، مدل رجیستری
  VibeFarsiUI.Infrastructure/ خواندن رجیستری JSON با DI/Options/ILogger
tests/
  VibeFarsiUI.UnitTests/      ۳۴ تست
  VibeFarsiUI.ComponentTests/ ۹ تست bUnit
docs/
scripts/                      صادرکردن رجیستری و سورس Razor
```

- **Interactive Server** با `@rendermode="InteractiveServer"`؛ تعامل‌های ساده (تم، tooltip، focus) با CSS/JS سبک.
- **بدون MudBlazor/Radzen/Syncfusion**؛ همه‌ی کامپوننت‌ها Razor بومی با توکن‌های CSS.
- **بدون React/Next.js در زمان اجرا**؛ سورس TSX فقط برای مرجع و بخش «کد» سایت نگه داشته شده است.

## کامپوننت‌های بومی

۳۵ نوع Razor در `src/VibeFarsiUI.Web/Components/UI`:

`Button`, `Input`, `Textarea`, `Select`, `Checkbox`, `Switch`, `RadioGroup`, `OtpField`, `NumberField`, `AmountInput`, `PhoneInput`, `NationalIdInput`, `SearchInput`, `Field`, `Badge`, `Card`, `Alert`, `Avatar`, `Kbd`, `Separator`, `Spinner`, `Skeleton`, `Progress`, `Stat`, `Price`, `EmptyState`, `Tabs`, `Accordion`, `AccordionItem`, `Tooltip`, `Rating`, `Pagination`, `Breadcrumb`, `Table`, `RelativeTime`, `CopyButton`, `JalaliDatePicker`.

ویژگی‌ها:

- API قوی‌تایپ: `[Parameter]`، `EventCallback<T>`، `@bind-Value`.
- پیش‌نمایش زنده در `/components/{slug}` + جدول پارامترها که با Reflection از `[Parameter]`ها ساخته می‌شود.
- کد و نمونه‌ی استفاده با شماره‌خط، هایلایت سبک و کپی.
- ورودی‌های ایرانی: نرمال‌سازی موبایل و اپراتور، کد ملی با اعتبارسنجی، مبلغ با جداکننده‌ی فارسی.
- تقویم شمسی با شنبه به‌عنوان اول هفته، تعطیلی جمعه، ناوبری ماه و انتخاب روز.

## فارسی و RTL

- `<html lang="fa" dir="rtl">` و locale فارسی در نمایش اعداد.
- فونت اصلی IRANSans (محلی) و Vazirmatn برای تم‌ها.
- CSS با ویژگی‌های منطقی (`margin-inline-*`, `inset-inline-*`, `border-inline-*`) و پرهیز از left/right ثابت.
- `PersianText`: تبدیل رقم فارسی/انگلیسی، `FaNumber` با جداکننده‌ی `٬`، `FormatToman`، درصد و حجم فایل.
- `JalaliCalendar` + `CalendarMath`: تبدیل دوطرفه، سال کبیسه، طول ماه، روز هفته، شبکه‌ی ماه و قالب‌بندی.
- `IranianValidation`: موبایل، شبا، کد ملی، کارت (Luhn)، پلاک، کد پستی، شناسه‌ی حقوقی و قبض.

## تم‌ها

شش سیستم طراحی در `wwwroot/css/themes.css` با توکن‌های CSS: **گرافیت، فیروزه، زعفران، انار، لاجورد، کاغذ**.
تغییر تم از هدر یا `/themes` بدون reload انجام می‌شود و در `localStorage` می‌ماند. کامپوننت‌ها فقط از توکن‌ها (`--brand`, `--background`, `--shape-*`, `--type-*`, ...) استفاده می‌کنند.

## صفحات

| مسیر | توضیح |
|---|---|
| `/` | صفحه‌ی اصلی فارسی |
| `/components` | کاتالوگ ۱۰۸ کامپوننت + جست‌وجو + فیلتر دسته |
| `/components/{slug}` | پیش‌نمایش زنده، پارامترها، کد، پرامپت، موارد مرتبط |
| `/blocks`, `/charts`, `/calendar`, `/animations`, `/backgrounds`, `/templates`, `/skills`, `/sites` | کاتالوگ بخش‌ها |
| `/{section}/{slug}` | جزئیات، سورس اصلی، پرامپت و متادیتا |
| `/themes` | گالری تعاملی تم‌ها |
| `/docs` | راهنمای نصب، معماری و رجیستری |
| `/icons` | گالری آیکون‌های Lucide درون‌خطی |
| `/r/catalog`, `/r/{type}/{slug}.json` | قرارداد ماشینی رجیستری |
| `/sitemap.xml`, `/robots.txt` | SEO |

SEO: عنوان و توضیح فارسی، canonical، Open Graph، sitemap و robots. مسیرهای ثابت اصلی حفظ شده‌اند.

## رجیستری و ابزارهای اصلی

رجیستری بدون بازسازی دستی از متادیتای TypeScript صادر شده است:

```bash
node scripts/export-registry.mjs         # رجیستری TS را به catalog.json و documents.json تبدیل می‌کند
node scripts/export-razor-source.mjs
node scripts/migration-status.mjs
```

- `wwwroot/registry/catalog.json`: ۳۹۲ آیتم در ۱۱ نوع (۱۰۸ کامپوننت، ۷۵ انیمیشن، ۵۴ پس‌زمینه، ۴۶ بلاک، ۴۲ قالب، ۱۸ نمودار، ۱۳ تقویم، ۱۳ مهارت، ۱۱ کتابخانه، ۶ تم، ۶ سایت).
- `documents.json`: همان شکل `RegistryDocument` اصلی برای `/r/{type}/{slug}.json`.
- CLI (`packages/cli`) و MCP (`mcp/`) دست‌نخورده و مستقل باقی مانده‌اند و می‌توانند همین قرارداد را مصرف کنند.

## تست

```bash
dotnet test VibeFarsiUI.sln
```

- ۳۴ تست واحد: تبدیل رقم و تومان، تقویم جلالی (کبیسه، شنبه، قالب‌بندی)، اعتبارسنجی ایرانی، بارگذاری و جست‌وجوی رجیستری.
- ۹ تست bUnit: Button، Checkbox، PhoneInput، Price، Alert، JalaliDatePicker (شامل حفظ ماه پس از re-render) و ComponentPreview.

## محدودیت‌ها و کارهای باقی‌مانده

- ۷۲ کامپوننت رجیستری هنوز Razor بومی ندارند (combobox، dialog، data-table، file-upload، date-range-picker، chart و…). صفحه‌ی آن‌ها متادیتا/سورس/پرامپت را نشان می‌دهد و صریحاً «در صف مهاجرت» است.
- پیش‌نمایش زنده‌ی بلاک‌ها، نمودارها، انیمیشن‌ها، پس‌زمینه‌ها و قالب‌ها هنوز پیاده‌سازی نشده است.
- بخش community/سندباکس و APIهای تحلیلی اصلی به این نسخه منتقل نشده‌اند.
- CLI و MCP هنوز Node.js هستند (طبق تصمیم مهاجرت، این ابزارها مستقل می‌مانند).

## مجوز

MIT — بر پایه‌ی پروژه‌ی اصلی VibeFarsiUI.
