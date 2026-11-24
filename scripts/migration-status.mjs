// Generates docs/COMPONENT_MIGRATION_STATUS.md from the exported registry and
// the Razor mapping (single source of truth).
import { readFileSync, writeFileSync } from "node:fs";

const catalog = JSON.parse(readFileSync("src/VibeFarsiUI.Web/wwwroot/registry/catalog.json", "utf8"));
const mappingSource = readFileSync("src/VibeFarsiUI.Web/Catalog/MigratedComponents.cs", "utf8");
const mapped = new Map();

const mapPattern = /\["([^"]+)"\] = new\("[^"]+", typeof\(([A-Za-z0-9_]+)\), "([^"]+)"/g;
for (const match of mappingSource.matchAll(mapPattern)) {
  mapped.set(match[1], { type: match[2], source: match[3] });
}

const components = catalog.items.filter((item) => item.type === "component");
const tested = new Set(["button", "price", "date-picker", "phone-input", "alert", "checkbox-group"]);

const escape = (value) => String(value ?? "").replaceAll("|", "\\|").replaceAll("\n", " ");
const filePath = (source) => `src/VibeFarsiUI.Web/Components/UI/${source}.razor`;

const rows = components.map((item, index) => {
  const native = mapped.get(item.slug);
  const status = native ? "Implemented" : "Not Started";
  const preview = native ? "✅ زنده" : "—";
  const test = tested.has(item.slug) ? "✅ bUnit" : native ? "SSR smoke" : "—";
  return `| ${index + 1} | ${escape(item.name)} | \`${item.slug}\` | \`${item.file}\` | ${native ? `\`${filePath(native.source)}\`` : "—"} | ${escape(item.deps?.join("، ") || "—")} | ${status} | ${preview} | ${test} |`;
});

const header = `# وضعیت مهاجرت کامپوننت‌ها

این سند از داده‌ی واقعی رجیستری (\`wwwroot/registry/catalog.json\`) و نگاشت \`MigratedComponents.cs\` تولید می‌شود.
برای به‌روزرسانی: \`node scripts/migration-status.mjs\`.

## خلاصه

| شاخص | مقدار |
|---|---|
| کل کامپوننت‌های رجیستری | ${components.length} |
| اسلاگ‌های دارای پیش‌نمایش بومی | ${mapped.size} |
| انواع کامپوننت Razor | ${new Set([...mapped.values()].map((x) => x.type)).size} |
| اسلاگ‌های تست‌شده با bUnit | ${tested.size} |
| تست‌های bUnit | 9 |
| در صف مهاجرت | ${components.length - mapped.size} |

> وضعیت «Implemented» یعنی پیاده‌سازی Razor و پیش‌نمایش زنده وجود دارد. فقط کامپوننت‌هایی که در ستون تست «bUnit» دارند به‌صورت خودکار تست شده‌اند؛ بقیه با دود‌تست SSR بررسی شده‌اند.

## فهرست کامپوننت‌ها

| # | نام | Slug | فایل اصلی | مسیر Blazor | وابستگی‌ها | وضعیت | پیش‌نمایش | تست |
|---|---|---|---|---|---|---|---|---|
${rows.join("\n")}

## سایر بخش‌های اکوسیستم

| نوع | تعداد | وضعیت سایت |
|---|---|---|
| بلاک‌ها | ${catalog.items.filter((i) => i.type === "block").length} | کاتالوگ و صفحه‌ی جزئیات با سورس اصلی و پرامپت |
| نمودارها | ${catalog.items.filter((i) => i.type === "chart").length} | کاتالوگ و صفحه‌ی جزئیات با سورس اصلی و پرامپت |
| تقویم | ${catalog.items.filter((i) => i.type === "calendar").length} | کاتالوگ + تقویم شمسی بومی |
| انیمیشن‌ها | ${catalog.items.filter((i) => i.type === "animation").length} | کاتالوگ و سورس |
| پس‌زمینه‌ها | ${catalog.items.filter((i) => i.type === "background").length} | کاتالوگ و سورس |
| قالب‌ها | ${catalog.items.filter((i) => i.type === "template").length} | کاتالوگ و سورس |
| تم‌ها | ${catalog.items.filter((i) => i.type === "theme").length} | گالری تعاملی + CSS توکن‌ها |
| مهارت‌ها | ${catalog.items.filter((i) => i.type === "skill").length} | فهرست + سورس Markdown و پرامپت |
| کتابخانه‌ها | ${catalog.items.filter((i) => i.type === "lib").length} | از طریق رجیستری \`/r/lib/...\` |
| سایت‌ها | ${catalog.items.filter((i) => i.type === "site").length} | کاتالوگ و سورس |

## محدودیت‌های باقی‌مانده

- پیش‌نمایش زنده برای بخش‌های غیر از کامپوننت‌ها (بلاک، نمودار، انیمیشن، پس‌زمینه، قالب) هنوز پیاده‌سازی نشده است؛ متادیتا، سورس و پرامپت در دسترس است.
- کامپوننت‌های فرم پیچیده (combobox، file-upload، data-table، date-range-picker و…) هنوز مهاجرت نشده‌اند.
- CLI و MCP همچنان نسخه‌ی Node.js اصلی هستند و از همان قرارداد \`/r\` استفاده می‌کنند.
`;

writeFileSync("docs/COMPONENT_MIGRATION_STATUS.md", header);
console.log(`Wrote docs/COMPONENT_MIGRATION_STATUS.md (${components.length} components, ${mapped.size} native slugs)`);
