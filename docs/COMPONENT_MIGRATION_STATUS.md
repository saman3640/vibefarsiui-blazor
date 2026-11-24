# وضعیت مهاجرت کامپوننت‌ها

این سند از داده‌ی واقعی رجیستری (`wwwroot/registry/catalog.json`) و نگاشت `MigratedComponents.cs` تولید می‌شود.
برای به‌روزرسانی: `node scripts/migration-status.mjs`.

## خلاصه

| شاخص | مقدار |
|---|---|
| کل کامپوننت‌های رجیستری | 108 |
| اسلاگ‌های دارای پیش‌نمایش بومی | 36 |
| انواع کامپوننت Razor | 35 |
| اسلاگ‌های تست‌شده با bUnit | 6 |
| تست‌های bUnit | 9 |
| در صف مهاجرت | 72 |

> وضعیت «Implemented» یعنی پیاده‌سازی Razor و پیش‌نمایش زنده وجود دارد. فقط کامپوننت‌هایی که در ستون تست «bUnit» دارند به‌صورت خودکار تست شده‌اند؛ بقیه با دود‌تست SSR بررسی شده‌اند.

## فهرست کامپوننت‌ها

| # | نام | Slug | فایل اصلی | مسیر Blazor | وابستگی‌ها | وضعیت | پیش‌نمایش | تست |
|---|---|---|---|---|---|---|---|---|
| 1 | دکمه | `button` | `registry/ui/button.tsx` | `src/VibeFarsiUI.Web/Components/UI/Button.razor` | lucide-react | Implemented | ✅ زنده | ✅ bUnit |
| 2 | ورودی | `input` | `registry/ui/input.tsx` | `src/VibeFarsiUI.Web/Components/UI/Input.razor` | — | Implemented | ✅ زنده | SSR smoke |
| 3 | متن چندخطی | `textarea` | `registry/ui/textarea.tsx` | `src/VibeFarsiUI.Web/Components/UI/Textarea.razor` | — | Implemented | ✅ زنده | SSR smoke |
| 4 | انتخاب | `select` | `registry/ui/select.tsx` | `src/VibeFarsiUI.Web/Components/UI/Select.razor` | lucide-react | Implemented | ✅ زنده | SSR smoke |
| 5 | کمبوباکس | `combobox` | `registry/ui/combobox.tsx` | — | lucide-react | Not Started | — | — |
| 6 | کد تأیید | `otp-field` | `registry/ui/otp-field.tsx` | `src/VibeFarsiUI.Web/Components/UI/OtpField.razor` | — | Implemented | ✅ زنده | SSR smoke |
| 7 | عدد | `number-field` | `registry/ui/number-field.tsx` | `src/VibeFarsiUI.Web/Components/UI/NumberField.razor` | lucide-react | Implemented | ✅ زنده | SSR smoke |
| 8 | گروه چک‌باکس | `checkbox-group` | `registry/ui/checkbox.tsx` | `src/VibeFarsiUI.Web/Components/UI/Checkbox.razor` | lucide-react | Implemented | ✅ زنده | ✅ bUnit |
| 9 | گروه رادیویی | `radio-group` | `registry/ui/radio-group.tsx` | `src/VibeFarsiUI.Web/Components/UI/RadioGroup.razor` | — | Implemented | ✅ زنده | SSR smoke |
| 10 | کلید | `switch` | `registry/ui/switch.tsx` | `src/VibeFarsiUI.Web/Components/UI/Switch.razor` | — | Implemented | ✅ زنده | SSR smoke |
| 11 | اسلایدر | `slider` | `registry/ui/slider.tsx` | — | — | Not Started | — | — |
| 12 | اسلایدر بازه | `range-slider` | `registry/ui/range-slider.tsx` | — | — | Not Started | — | — |
| 13 | امتیاز | `rating` | `registry/ui/rating.tsx` | `src/VibeFarsiUI.Web/Components/UI/Rating.razor` | lucide-react | Implemented | ✅ زنده | SSR smoke |
| 14 | آپلود فایل | `file-upload` | `registry/ui/file-upload.tsx` | — | lucide-react | Not Started | — | — |
| 15 | تقویم شمسی | `calendar` | `registry/ui/calendar.tsx` | `src/VibeFarsiUI.Web/Components/UI/JalaliDatePicker.razor` | lucide-react | Implemented | ✅ زنده | SSR smoke |
| 16 | انتخاب تاریخ | `date-picker` | `registry/ui/date-picker.tsx` | `src/VibeFarsiUI.Web/Components/UI/JalaliDatePicker.razor` | lucide-react | Implemented | ✅ زنده | ✅ bUnit |
| 17 | پالت دستور | `command` | `registry/ui/command.tsx` | — | lucide-react | Not Started | — | — |
| 18 | پنجره | `dialog` | `registry/ui/dialog.tsx` | — | lucide-react | Not Started | — | — |
| 19 | تأیید عمل | `alert-dialog` | `registry/ui/alert-dialog.tsx` | — | — | Not Started | — | — |
| 20 | منوی کشویی | `dropdown-menu` | `registry/ui/dropdown-menu.tsx` | — | — | Not Started | — | — |
| 21 | راهنمای ابزار | `tooltip` | `registry/ui/tooltip.tsx` | `src/VibeFarsiUI.Web/Components/UI/Tooltip.razor` | — | Implemented | ✅ زنده | SSR smoke |
| 22 | کشو | `sheet` | `registry/ui/sheet.tsx` | — | lucide-react | Not Started | — | — |
| 23 | تب‌ها | `tabs` | `registry/ui/tabs.tsx` | `src/VibeFarsiUI.Web/Components/UI/Tabs.razor` | — | Implemented | ✅ زنده | SSR smoke |
| 24 | صفحه‌بندی | `pagination` | `registry/ui/pagination.tsx` | `src/VibeFarsiUI.Web/Components/UI/Pagination.razor` | lucide-react | Implemented | ✅ زنده | SSR smoke |
| 25 | مسیر | `breadcrumb` | `registry/ui/breadcrumb.tsx` | `src/VibeFarsiUI.Web/Components/UI/Breadcrumb.razor` | lucide-react | Implemented | ✅ زنده | SSR smoke |
| 26 | مراحل | `stepper` | `registry/ui/stepper.tsx` | — | lucide-react | Not Started | — | — |
| 27 | نوار کناری | `sidebar` | `registry/ui/sidebar.tsx` | — | lucide-react | Not Started | — | — |
| 28 | اعلان | `toast` | `registry/ui/toast.tsx` | — | lucide-react | Not Started | — | — |
| 29 | هشدار | `alert` | `registry/ui/alert.tsx` | `src/VibeFarsiUI.Web/Components/UI/Alert.razor` | lucide-react | Implemented | ✅ زنده | ✅ bUnit |
| 30 | پیشرفت | `progress` | `registry/ui/progress.tsx` | `src/VibeFarsiUI.Web/Components/UI/Progress.razor` | — | Implemented | ✅ زنده | SSR smoke |
| 31 | اسکلت | `skeleton` | `registry/ui/skeleton.tsx` | `src/VibeFarsiUI.Web/Components/UI/Skeleton.razor` | — | Implemented | ✅ زنده | SSR smoke |
| 32 | حالت خالی | `empty-state` | `registry/ui/empty-state.tsx` | `src/VibeFarsiUI.Web/Components/UI/EmptyState.razor` | lucide-react | Implemented | ✅ زنده | SSR smoke |
| 33 | نشان | `badge` | `registry/ui/badge.tsx` | `src/VibeFarsiUI.Web/Components/UI/Badge.razor` | — | Implemented | ✅ زنده | SSR smoke |
| 34 | آواتار | `avatar` | `registry/ui/avatar.tsx` | `src/VibeFarsiUI.Web/Components/UI/Avatar.razor` | — | Implemented | ✅ زنده | SSR smoke |
| 35 | جدول | `table` | `registry/ui/table.tsx` | `src/VibeFarsiUI.Web/Components/UI/Table.razor` | — | Implemented | ✅ زنده | SSR smoke |
| 36 | آمار | `stat` | `registry/ui/stat.tsx` | `src/VibeFarsiUI.Web/Components/UI/Stat.razor` | lucide-react | Implemented | ✅ زنده | SSR smoke |
| 37 | قیمت | `price` | `registry/ui/price.tsx` | `src/VibeFarsiUI.Web/Components/UI/Price.razor` | — | Implemented | ✅ زنده | ✅ bUnit |
| 38 | خط زمان | `timeline` | `registry/ui/timeline.tsx` | — | — | Not Started | — | — |
| 39 | آکاردئون | `accordion` | `registry/ui/accordion.tsx` | `src/VibeFarsiUI.Web/Components/UI/Accordion.razor` | lucide-react | Implemented | ✅ زنده | SSR smoke |
| 40 | کلید میانبر | `kbd` | `registry/ui/kbd.tsx` | `src/VibeFarsiUI.Web/Components/UI/Kbd.razor` | — | Implemented | ✅ زنده | SSR smoke |
| 41 | جعبه‌ی پرامپت | `prompt-input` | `registry/ui/prompt-input.tsx` | — | lucide-react | Not Started | — | — |
| 42 | کارت | `card` | `registry/ui/card.tsx` | `src/VibeFarsiUI.Web/Components/UI/Card.razor` | — | Implemented | ✅ زنده | SSR smoke |
| 43 | جدول داده | `data-table` | `registry/ui/data-table.tsx` | — | lucide-react | Not Started | — | — |
| 44 | نمودار | `chart` | `registry/ui/chart.tsx` | — | — | Not Started | — | — |
| 45 | پاپ‌آور | `popover` | `registry/ui/popover.tsx` | — | — | Not Started | — | — |
| 46 | منوی راست‌کلیک | `context-menu` | `registry/ui/context-menu.tsx` | — | — | Not Started | — | — |
| 47 | کارت شناور | `hover-card` | `registry/ui/hover-card.tsx` | — | — | Not Started | — | — |
| 48 | اسلایدر تصویر | `carousel` | `registry/ui/carousel.tsx` | — | lucide-react | Not Started | — | — |
| 49 | کمبوباکس آنلاین | `combobox-async` | `registry/ui/combobox-async.tsx` | — | lucide-react | Not Started | — | — |
| 50 | رمز عبور | `password-input` | `registry/ui/password-input.tsx` | — | lucide-react | Not Started | — | — |
| 51 | شماره‌ی شبا | `iban-input` | `registry/ui/iban-input.tsx` | — | lucide-react | Not Started | — | — |
| 52 | شماره‌ی موبایل | `phone-input` | `registry/ui/phone-input.tsx` | `src/VibeFarsiUI.Web/Components/UI/PhoneInput.razor` | lucide-react | Implemented | ✅ زنده | ✅ bUnit |
| 53 | صندوق اعلان | `notification-inbox` | `registry/ui/notification-inbox.tsx` | — | lucide-react | Not Started | — | — |
| 54 | فرم | `form` | `registry/ui/form.tsx` | — | — | Not Started | — | — |
| 55 | کد ملی | `national-id-input` | `registry/ui/national-id-input.tsx` | `src/VibeFarsiUI.Web/Components/UI/NationalIdInput.razor` | lucide-react | Implemented | ✅ زنده | SSR smoke |
| 56 | شماره‌ی کارت | `card-number-input` | `registry/ui/card-number-input.tsx` | — | lucide-react | Not Started | — | — |
| 57 | کارت به کارت | `card-to-card` | `registry/ui/card-to-card.tsx` | — | lucide-react | Not Started | — | — |
| 58 | پلاک خودرو | `plate-input` | `registry/ui/plate-input.tsx` | — | — | Not Started | — | — |
| 59 | استان و شهر | `address-picker` | `registry/ui/address-picker.tsx` | — | lucide-react | Not Started | — | — |
| 60 | کد پستی | `postal-code-input` | `registry/ui/postal-code-input.tsx` | — | lucide-react | Not Started | — | — |
| 61 | بازه‌ی تاریخ شمسی | `date-range-picker` | `registry/ui/date-range-picker.tsx` | — | lucide-react | Not Started | — | — |
| 62 | انتخاب ساعت | `time-picker` | `registry/ui/time-picker.tsx` | — | lucide-react | Not Started | — | — |
| 63 | مبلغ | `amount-input` | `registry/ui/amount-input.tsx` | `src/VibeFarsiUI.Web/Components/UI/AmountInput.razor` | — | Implemented | ✅ زنده | SSR smoke |
| 64 | جست‌وجو | `search-input` | `registry/ui/search-input.tsx` | `src/VibeFarsiUI.Web/Components/UI/SearchInput.razor` | lucide-react | Implemented | ✅ زنده | SSR smoke |
| 65 | برچسب‌ها | `tags-input` | `registry/ui/tags-input.tsx` | — | lucide-react | Not Started | — | — |
| 66 | چندانتخابی | `multi-select` | `registry/ui/multi-select.tsx` | — | lucide-react | Not Started | — | — |
| 67 | دکمه‌ی فشاری | `toggle` | `registry/ui/toggle.tsx` | — | — | Not Started | — | — |
| 68 | کنترل بخشی | `segmented-control` | `registry/ui/segmented-control.tsx` | — | — | Not Started | — | — |
| 69 | جداکننده | `separator` | `registry/ui/separator.tsx` | `src/VibeFarsiUI.Web/Components/UI/Separator.razor` | — | Implemented | ✅ زنده | SSR smoke |
| 70 | بارگذاری | `spinner` | `registry/ui/spinner.tsx` | `src/VibeFarsiUI.Web/Components/UI/Spinner.razor` | — | Implemented | ✅ زنده | SSR smoke |
| 71 | بازشو | `collapsible` | `registry/ui/collapsible.tsx` | — | lucide-react | Not Started | — | — |
| 72 | ناحیه اسکرول | `scroll-area` | `registry/ui/scroll-area.tsx` | — | — | Not Started | — | — |
| 73 | شمارش معکوس | `countdown` | `registry/ui/countdown.tsx` | — | — | Not Started | — | — |
| 74 | پخش‌کننده ویدئو | `video-player` | `registry/ui/video-player.tsx` | — | lucide-react | Not Started | — | — |
| 75 | سرفصل دوره | `course-outline` | `registry/ui/course-outline.tsx` | — | lucide-react | Not Started | — | — |
| 76 | درس‌نامه | `lesson-note` | `registry/ui/lesson-note.tsx` | — | lucide-react | Not Started | — | — |
| 77 | نمودار تعاملی | `function-plot` | `registry/ui/function-plot.tsx` | — | lucide-react | Not Started | — | — |
| 78 | شکل تعاملی | `hotspot-figure` | `registry/ui/hotspot-figure.tsx` | — | lucide-react | Not Started | — | — |
| 79 | ارزیابی | `quiz` | `registry/ui/quiz.tsx` | — | lucide-react | Not Started | — | — |
| 80 | پیام چت | `chat-message` | `registry/ui/chat-message.tsx` | — | lucide-react | Not Started | — | — |
| 81 | مارک‌داون | `markdown` | `registry/ui/markdown.tsx` | — | — | Not Started | — | — |
| 82 | بلوک کد | `code-block` | `registry/ui/code-block.tsx` | — | lucide-react | Not Started | — | — |
| 83 | فراخوانی ابزار | `tool-call` | `registry/ui/tool-call.tsx` | — | lucide-react | Not Started | — | — |
| 84 | منابع و ارجاع | `sources` | `registry/ui/sources.tsx` | — | lucide-react | Not Started | — | — |
| 85 | انتخاب مدل | `model-picker` | `registry/ui/model-picker.tsx` | — | lucide-react | Not Started | — | — |
| 86 | تاریخ چرخشی | `wheel-date-picker` | `registry/ui/wheel-date-picker.tsx` | — | — | Not Started | — | — |
| 87 | تاریخ تولد | `birthday-picker` | `registry/ui/birthday-picker.tsx` | — | — | Not Started | — | — |
| 88 | شناسه‌ی قبض | `bill-input` | `registry/ui/bill-input.tsx` | — | lucide-react | Not Started | — | — |
| 89 | شناسه‌ی ملی | `legal-id-input` | `registry/ui/legal-id-input.tsx` | — | lucide-react | Not Started | — | — |
| 90 | محاسبه‌گر اقساط | `installment-calculator` | `registry/ui/installment-calculator.tsx` | — | — | Not Started | — | — |
| 91 | تومان و ریال | `currency-toggle` | `registry/ui/currency-toggle.tsx` | — | — | Not Started | — | — |
| 92 | زمان نسبی | `relative-time` | `registry/ui/relative-time.tsx` | `src/VibeFarsiUI.Web/Components/UI/RelativeTime.razor` | — | Implemented | ✅ زنده | SSR smoke |
| 93 | روش ارسال | `shipping-methods` | `registry/ui/shipping-methods.tsx` | — | lucide-react | Not Started | — | — |
| 94 | روش پرداخت | `payment-methods` | `registry/ui/payment-methods.tsx` | — | lucide-react | Not Started | — | — |
| 95 | دکمه‌ی کپی | `copy-button` | `registry/ui/copy-button.tsx` | `src/VibeFarsiUI.Web/Components/UI/CopyButton.razor` | lucide-react | Implemented | ✅ زنده | SSR smoke |
| 96 | نوار پایین موبایل | `bottom-nav` | `registry/ui/bottom-nav.tsx` | — | lucide-react | Not Started | — | — |
| 97 | کشوی پایین | `drawer` | `registry/ui/drawer.tsx` | — | — | Not Started | — | — |
| 98 | درخت | `tree-view` | `registry/ui/tree-view.tsx` | — | lucide-react | Not Started | — | — |
| 99 | فهرست مرتب‌شدنی | `sortable-list` | `registry/ui/sortable-list.tsx` | — | lucide-react | Not Started | — | — |
| 100 | پنل‌های قابل تغییر اندازه | `resizable` | `registry/ui/resizable.tsx` | — | — | Not Started | — | — |
| 101 | انتخاب رنگ | `color-picker` | `registry/ui/color-picker.tsx` | — | lucide-react | Not Started | — | — |
| 102 | برش تصویر | `image-cropper` | `registry/ui/image-cropper.tsx` | — | lucide-react | Not Started | — | — |
| 103 | امضا | `signature-pad` | `registry/ui/signature-pad.tsx` | — | lucide-react | Not Started | — | — |
| 104 | کد QR | `qr-code` | `registry/ui/qr-code.tsx` | — | — | Not Started | — | — |
| 105 | منشن | `mention-input` | `registry/ui/mention-input.tsx` | — | — | Not Started | — | — |
| 106 | تور معرفی | `product-tour` | `registry/ui/product-tour.tsx` | — | lucide-react | Not Started | — | — |
| 107 | نوار منو | `menubar` | `registry/ui/menubar.tsx` | — | lucide-react | Not Started | — | — |
| 108 | منوی ناوبری سایت | `navigation-menu` | `registry/ui/navigation-menu.tsx` | — | lucide-react | Not Started | — | — |

## سایر بخش‌های اکوسیستم

| نوع | تعداد | وضعیت سایت |
|---|---|---|
| بلاک‌ها | 46 | کاتالوگ و صفحه‌ی جزئیات با سورس اصلی و پرامپت |
| نمودارها | 18 | کاتالوگ و صفحه‌ی جزئیات با سورس اصلی و پرامپت |
| تقویم | 13 | کاتالوگ + تقویم شمسی بومی |
| انیمیشن‌ها | 75 | کاتالوگ و سورس |
| پس‌زمینه‌ها | 54 | کاتالوگ و سورس |
| قالب‌ها | 42 | کاتالوگ و سورس |
| تم‌ها | 6 | گالری تعاملی + CSS توکن‌ها |
| مهارت‌ها | 13 | فهرست + سورس Markdown و پرامپت |
| کتابخانه‌ها | 11 | از طریق رجیستری `/r/lib/...` |
| سایت‌ها | 6 | کاتالوگ و سورس |

## محدودیت‌های باقی‌مانده

- پیش‌نمایش زنده برای بخش‌های غیر از کامپوننت‌ها (بلاک، نمودار، انیمیشن، پس‌زمینه، قالب) هنوز پیاده‌سازی نشده است؛ متادیتا، سورس و پرامپت در دسترس است.
- کامپوننت‌های فرم پیچیده (combobox، file-upload، data-table، date-range-picker و…) هنوز مهاجرت نشده‌اند.
- CLI و MCP همچنان نسخه‌ی Node.js اصلی هستند و از همان قرارداد `/r` استفاده می‌کنند.
