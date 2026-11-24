# VibeFarsiUI — Migration Analysis (Next.js → .NET 10 Blazor)

Status: Phase 1 complete  
Target fork: `saman3640/vibefarsiui-blazor`  
Reference upstream: `TronIsHere/vibefarsiui`  
Analysis date: 2026-11-24

## 1. Repository and Git baseline

| Item | Finding |
| --- | --- |
| Local branch | `main` |
| Working tree | Clean except untracked `.freebuff/` agent skills |
| `origin` | `https://github.com/saman3640/vibefarsiui-blazor.git` (correct fork) |
| `upstream` | Not configured |
| History | Preserved; latest commit `889cccf` (merge PR #27 from `TronIsHere/dev`) |
| .NET SDKs | 9.0.101, 10.0.404, 11.0.100 (10.0.404 pinned by `global.json`) |
| Node (tooling only) | v26.13.0 |

Recommended upstream remote (not executed automatically):

```bash
git remote add upstream https://github.com/TronIsHere/vibefarsiui.git
git fetch upstream
```

No force-push, no rewrite of existing commits. Migration work will use feature branches per phase.

## 2. Existing architecture

```
app/                     Next.js App Router pages and route handlers
components/              Site chrome, docs shell, landing, demos
registry/                Source of truth: .tsx components, .css themes, .md skills, site folders
lib/registry/            Metadata for every registry item + prompt builder + catalog
lib/                     Persian helpers, Jalali, validation, charts, holidays, community, analytics
packages/cli/            `vibefarsi` npm CLI (TypeScript, compiled to dist/)
mcp/                     `@vibefarsi/mcp` stdio server (TypeScript)
public/                  Images, previews, icons, demo assets
scripts/                 Preview capture, theme file build, calendar checks
```

### Request paths

| Path | Purpose |
| --- | --- |
| `/` | Landing page |
| `/components`, `/components/[slug]` | Component catalog and detail pages |
| `/blocks`, `/charts`, `/calendar`, `/animations`, `/backgrounds`, `/templates`, `/sites`, `/skills`, `/themes` (+ `[slug]`) | Section catalogs and detail pages |
| `/docs`, `/about`, `/icons`, `/showcase`, `/showcase/submit`, `/community`, `/community/[id]`, `/community/submit` | Documentation and community pages |
| `/preview/[slug]`, `/preview/block/[slug]`, `/preview/site/[slug]/[[...path]]` | Isolated full-page component previews |
| `/r`, `/r/catalog`, `/r/[type]/[slug].json` | Machine-readable registry consumed by CLI and MCP |
| `/mcp` | HTTP MCP endpoint |
| `/api/a`, `/api/community/*` | Analytics ingest and community storage |
| `/sandbox` | Community code playground (CSP-sandboxed) |
| `/llms.txt`, `/llms-full.txt`, `/sitemap.xml`, `/robots.txt` | SEO / AI discovery |

### Stack and dependencies

- Next.js 16.3.5, React 19.2.8, TypeScript 5, Tailwind CSS v4.
- Rendering: React Server Components; interactive islands are `"use client"`.
- `lucide-react` for all registry icons.
- `shiki` for server-side syntax highlighting; CodeMirror for the sandbox editor.
- `sucrase` to compile community JSX in the sandbox.
- `zod` for MCP input schemas.
- `@modelcontextprotocol/server` for MCP.
- No component library beyond Radix-style hand-written primitives; registry components are copied source, not npm packages.
- Persistence: community/analytics use local/file-backed stores (no external database configured).

## 3. Complete feature inventory

| Feature | Source | Notes |
| --- | --- | --- |
| Component registry | `registry/**`, `lib/registry/**` | 392 metadata items across 11 types |
| Registry HTTP API | `app/r/**` | catalog + per-item JSON with source and prompt |
| Component explorer | `app/(docs)/*` | category filters, search, detail pages with tabs (preview / code / prompt) |
| Live demos | `components/demos/**` | 15 demo aggregator files mapping slugs → React demos |
| Persian formatting | `lib/utils.ts`, `lib/number-to-words.ts` | digits, separators, Toman, percent, file size, number-to-words |
| Iranian validation | `lib/persian.ts` | mobile, IBAN, national/legal ID, card/Luhn, plate, postal code, bill/payment IDs |
| Jalali calendar | `lib/jalali.ts`, `lib/calendar-utils.ts`, `lib/hijri.ts`, `lib/iran-holidays.ts` | conversion, month/week helpers, Hijri occasions, official holidays |
| Charts | `registry/charts/**` | 18 dependency-free SVG/HTML charts + shared chart core |
| Calendar views | `registry/calendar/**` | 13 views: month/week/day/agenda/year, booking, Gantt, shifts, prayer times |
| Animations | `registry/animations/**` | 75 CSS/React animations, no Framer Motion |
| Backgrounds | `registry/backgrounds/**` | 54 CSS/WebGL backgrounds (shader primitive) |
| Blocks | `registry/blocks/**` | 46 page sections |
| Templates | `registry/templates/**` | 42 full pages |
| Sites | `registry/sites/**` | 6 multi-page websites |
| Themes | `registry/themes/*.css`, `app/globals.css` | 6 design systems: graphite, turquoise, saffron, pomegranate, lapis, paper |
| Skills | `registry/skills/**` | 13 agent skills/guides in Markdown |
| Icons page | `app/(docs)/icons/page.tsx` | Lucide icon browser |
| Showcase / community | `app/(docs)/showcase`, `app/(docs)/community`, `app/api/community` | submissions, media, detail pages |
| Sandbox | `app/sandbox`, `registry/sandbox-modules.ts`, `lib/sandbox-libs.ts` | in-browser JSX playground |
| SEO | `lib/site.ts`, `sitemap.ts`, `robots.ts`, `llms*.txt` | metadata, JSON-LD, canonical, Open Graph |
| CLI | `packages/cli/src/**` | `init`, `add`, `list`, registry client, CSS/agents setup |
| MCP | `mcp/src/**` | 5 tools: design rules, search, get component, get theme, scaffold |
| Preview capture | `scripts/capture-previews.mjs` | Playwright screenshots into `public/previews` |

## 4. Complete component inventory

### 4.1 UI components — 108 registered items

`accordion, address-picker, alert, alert-dialog, amount-input, avatar, badge, bill-input, birthday-picker, bottom-nav, breadcrumb, button, calendar, card, card-number-input, card-to-card, carousel, chart, chat-message, checkbox-group, code-block, collapsible, color-picker, combobox, combobox-async, command, context-menu, copy-button, countdown, course-outline, currency-toggle, data-table, date-picker, date-range-picker, dialog, drawer, dropdown-menu, empty-state, file-upload, form, function-plot, hotspot-figure, hover-card, iban-input, image-cropper, input, installment-calculator, kbd, legal-id-input, lesson-note, markdown, mention-input, menubar, model-picker, multi-select, national-id-input, navigation-menu, notification-inbox, number-field, otp-field, pagination, password-input, payment-methods, phone-input, plate-input, popover, postal-code-input, price, product-tour, progress, prompt-input, qr-code, quiz, radio-group, range-slider, rating, relative-time, resizable, scroll-area, search-input, segmented-control, select, separator, sheet, shipping-methods, sidebar, signature-pad, skeleton, slider, sortable-list, sources, spinner, stat, stepper, switch, table, tabs, tags-input, textarea, time-picker, timeline, toast, toggle, tool-call, tooltip, tree-view, video-player, wheel-date-picker`

### 4.2 Other registry types

| Type | Count | Files |
| --- | --- | --- |
| UI components | 108 | `registry/ui/*.tsx` |
| Animations | 75 | `registry/animations/*.tsx` |
| Backgrounds | 54 | `registry/backgrounds/*.tsx` (+ WebGL shader entries) |
| Blocks | 46 | `registry/blocks/*.tsx` |
| Templates | 42 | `registry/templates/*.tsx` |
| Charts | 18 | `registry/charts/*.tsx` |
| Calendar | 13 | `registry/calendar/*.tsx` |
| Sites | 6 folders | `registry/sites/*` |
| Themes | 6 | `registry/themes/*.css` |
| Skills | 13 | `registry/skills/**` |
| Libs | 11 | `lib/registry/libs.ts` → `lib/*` helpers |
| **Total** | **392** | 412 registry files |

### 4.3 Demo/preview layer

`components/demos/` contains 15 aggregator modules (`components.tsx`, `components-general.tsx`, `components-persian.tsx`, `components-ai.tsx`, `education.tsx`, `charts.tsx`, `charts-extra.tsx`, `calendar.tsx`, `calendar-extra.tsx`, `blocks.tsx`, `blocks-extra.tsx`, `animations.tsx`, `backgrounds.tsx`, `templates.tsx`, `sites.tsx`). Each maps a registry slug to a live React demo. Every detail page renders the real component, not a screenshot.

## 5. Design system inventory

- `app/globals.css` (976 lines) defines:
  - Primitive/semantic color tokens in OKLCH (`--background`, `--foreground`, `--primary`, `--brand`, `--success`, `--warning`, `--field`, …).
  - Shape/line/depth/motion/density tokens (`--shape-*`, `--line`, `--depth-*`, `--press`, `--motion`, `--density`).
  - Tailwind `@theme inline` mapping (to be replaced by native CSS custom properties).
  - Base typography, focus rings, scrollbars, keyframes.
- Theme palettes live in `registry/themes/{graphite,turquoise,saffron,pomegranate,lapis,paper}.css`.
- Typography: local IRANSans (300/400/600/700) as primary, Vazirmatn and theme display faces (Lalezar, Noto Naskh, Noto Kufi, IBM Plex Sans Arabic) via Google Fonts, Geist Mono for code.
- RTL is default: `html lang="fa" dir="rtl"`, logical properties throughout (`ms/me`, `ps/pe`, `start/end`).
- Theme switching: `data-theme` on `<html>`, persisted in `localStorage` under `THEME_STORAGE_KEY`; `data-ds` scopes a full design language to previews.

## 6. React → Blazor mapping

| React pattern | Blazor equivalent |
| --- | --- |
| Props / `interface Props` | `[Parameter]` properties (strongly typed, `[EditorRequired]` where needed) |
| `children` | `RenderFragment ChildContent` |
| Named slots | `RenderFragment` parameters (`Header`, `Footer`, …) |
| Callbacks (`onChange`, `onComplete`) | `EventCallback<T>` / `EventCallback` |
| `useState` | Private fields + `StateHasChanged` |
| `useEffect` | `OnInitialized`, `OnParametersSet`, `OnAfterRenderAsync`, `IAsyncDisposable` |
| Context provider | `CascadingValue` / scoped DI service |
| Refs / DOM measurement | `ElementReference` + `IJSRuntime` (only where CSS cannot do it) |
| `"use client"` island | Interactive Server component (`@rendermode InteractiveServer`) |
| Conditional class joining (`cn`) | `CssBuilder`/`ClassNames` helper in Core |
| Tailwind utility classes | Semantic CSS classes backed by design tokens |
| `lucide-react` | Inline Lucide SVG components in `Components/UI/Icons` |
| `shiki` highlighting | Pre-rendered `<pre><code>` + CSS token colors (or Prism-free custom highlighter) |
| `next/font` | `@font-face` in CSS + `wwwroot/fonts` |

## 7. Proposed .NET architecture

```
VibeFarsiUI.sln
src/
  VibeFarsiUI.Core/            Persian text, numbers, jalali, validation, domain models
  VibeFarsiUI.Infrastructure/  Registry provider, JSON catalog, content/source access
  VibeFarsiUI.Web/             Blazor Web App (Interactive Server)
    Components/
      Layout/  Pages/  Shared/  UI/  UI/Icons/
    Features/
      Components/ Blocks/ Charts/ Calendar/ Animations/ Backgrounds/
      Templates/ Themes/ Skills/ Registry/
    wwwroot/  (css, fonts, images, registry data)
tests/
  VibeFarsiUI.UnitTests/       domain + registry + formatting
  VibeFarsiUI.ComponentTests/  bUnit component rendering
docs/
scripts/
Dockerfile
```

Rendering model: Blazor Web App with global Interactive Server, selective `@rendermode` for static pages to avoid unnecessary circuits. Registry data is read once at startup into a singleton provider.

## 8. Migration risks

| Risk | Impact | Mitigation |
| --- | --- | --- |
| 392 items / 412 registry files is very large | Long migration, easy to drift | Metadata-driven catalog export; migrate by category with tracked status |
| Visual identity depends on Tailwind utilities | Layout/colors can drift | Token-first CSS; port token file verbatim; visual review per component |
| 75 animations rely on React state/effects | Behavior loss | CSS keyframes + minimal JS interop; document differences |
| 54 backgrounds include WebGL shaders | Canvas/shader port | Port shader runtime to a JS module; wrap in Blazor component |
| Community sandbox uses CodeMirror + sucrase | Large JS surface | Keep as isolated static page + JS module; defer if not core |
| Analytics/community persistence is file-backed | Server coupling | Abstract store; keep in-memory/file provider |
| CLI/MCP contracts | Breaking clients | Preserve JSON shape and URLs exactly; validate with existing MCP smoke test |
| Jalali/Hijri edge cases | Wrong dates | Port algorithm 1:1, unit-test leap years and known holidays |
| Fonts licensing/size | Asset weight | Keep IRANSans files, subset if needed |
| SEO parity | Lost indexing | Port metadata, sitemap, robots, JSON-LD, canonical URLs |
| No Tailwind build in .NET | Huge manual CSS | Semantic component CSS + tokens; no utility-class parity required |

## 9. Implementation phases

| Phase | Scope | Exit criteria |
| --- | --- | --- |
| 1 | Analysis (this document) | Inventory, mapping, risks, architecture documented |
| 2 | .NET 10 solution foundation | `dotnet build` passes, DI/options/logging wired |
| 3 | Persian/RTL foundation | `fa-IR`, RTL, Vazirmatn/IRANSans, logical CSS |
| 4 | Formatting + Jalali | Core APIs + xUnit tests pass |
| 5 | Themes/design tokens | Six themes switch without reload |
| 6 | Component migration | Every UI item has a native Razor implementation + preview |
| 7 | Website pages | All catalogs, detail pages, search/filter, docs |
| 8 | Registry integration | `/r/catalog` and `/r/{type}/{slug}.json` parity |
| 9 | CLI/MCP compatibility | Existing Node tools work against new endpoint |
| 10 | Testing/optimisation | Unit + component tests, accessibility, performance |
| 11 | Docs/deployment | README, Dockerfile, final report |

## 10. Acceptance criteria

- .NET 10 solution builds and runs with `dotnet run` and Visual Studio 2026.
- Website runs with no Next.js/React at runtime.
- All pages and all 392 registry items are represented; every UI component has a native Razor implementation (target; current status is tracked per item in `COMPONENT_MIGRATION_STATUS.md`).
- Previews are live components, never static mockups.
- Six themes, RTL, Persian digits, Toman, Jalali calendar work.
- `/r/**` remains contract-compatible with the CLI and MCP.
- `dotnet test` passes; browser verification performed.
- Documentation: `MIGRATION_ANALYSIS.md`, `COMPONENT_MIGRATION_STATUS.md`, README/Docker instructions.

## 11. Registry contract (must be preserved)

`GET /r/catalog` returns:

```json
{
  "name": "vibefarsi",
  "homepage": "https://vibefarsi.ir",
  "schema": "https://vibefarsi.dev/schema/registry-item.json",
  "items": [
    {
      "type": "component",
      "slug": "button",
      "name": "دکمه",
      "desc": "…",
      "file": "registry/ui/button.tsx",
      "target": "components/ui/button.tsx",
      "url": "/r/components/button.json",
      "deps": ["lucide-react"],
      "registryDeps": [],
      "tags": ["form"],
      "category": "form",
      "aliases": ["button", "دکمه", "btn"],
      "prompt": "…"
    }
  ]
}
```

`GET /r/{type}/{slug}.json` returns `$schema`, `name`, `type`, `title`, `description`, `dependencies`, `registryDependencies`, `files[{path, content, type}]`, optional `css`, optional `prompt`.

Blazor will serve the same shapes from a generated catalog file plus migrated source files. The Node CLI and MCP server remain untouched and keep pointing at the same URLs.
