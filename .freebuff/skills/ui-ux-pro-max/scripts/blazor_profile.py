"""Blazor implementation profile for generated design systems.

The visual direction still comes from the existing design intelligence
(products, styles, palettes, typography). This module only maps that result
onto Blazor's hosting models, styling pipeline and the component libraries the
project may already use. It invents no colors: every value is read from the
generated palette, with neutral fallbacks only when a role is missing.
"""

BLAZOR_FRAMEWORK = "blazor"
BLAZOR_BREAKPOINTS = ("375px", "390px", "768px", "1024px", "1280px", "1440px")


def _color(colors: dict, key: str, fallback: str) -> str:
    return colors.get(key) or fallback


def _emitted_css_vars(design_system: dict) -> set:
    """Custom properties `--tokens css` actually emits for this design system."""
    from design_system import _flatten_tokens, build_design_tokens

    tokens = build_design_tokens(design_system)
    return set(_flatten_tokens(tokens.get("semantic") or {}, tokens, []))


def _var(name: str, emitted: set, fallback: str) -> str:
    """Reference a token only when the export contains it, else use the value."""
    return f"var({name})" if name in emitted else fallback


def format_blazor_implementation(design_system: dict) -> str:
    """Return markdown guidance for implementing this system in Blazor."""
    colors = design_system.get("colors") or {}
    typography = design_system.get("typography") or {}

    primary = _color(colors, "primary", "#2563eb")
    secondary = _color(colors, "secondary", "#64748b")
    accent = _color(colors, "accent", primary)
    background = _color(colors, "background", "#f8fafc")
    surface = _color(colors, "card", "#ffffff")
    foreground = _color(colors, "foreground", "#0f172a")
    muted_foreground = _color(colors, "muted_foreground", "#64748b")
    border = _color(colors, "border", "#e2e8f0")
    destructive = _color(colors, "destructive", "#dc2626")
    if destructive is None:
        destructive = "#dc2626"
    heading = typography.get("heading") or "system font stack"
    body = typography.get("body") or "system font stack"
    breakpoints = ", ".join(BLAZOR_BREAKPOINTS)

    # Only reference variables the token export really contains: a missing name
    # renders as an invalid declaration and silently drops the styling.
    emitted = _emitted_css_vars(design_system)
    # The resolved mode can say "light" while the chosen palette is a dark ramp
    # (dark-first products), so trust the palette's own background as well.
    from design_system import _palette_is_dark

    dark_system = (
        str(design_system.get("color_mode") or "light").lower() == "dark"
        or _palette_is_dark({"Background": background})
    )
    primary_ref = _var("--color-primary", emitted, primary)
    background_ref = _var("--color-background", emitted, background)
    foreground_ref = _var("--color-foreground", emitted, foreground)
    focus_ref = _var("--color-ring", emitted, primary_ref)
    spacing_md = _var("--space-md", emitted, "16px")
    spacing_lg = _var("--space-lg", emitted, "24px")
    card_declarations = []
    if "--color-card" in emitted:
        card_declarations.append(f"background: var(--color-card);")
    if "--color-border" in emitted:
        card_declarations.append(f"border: 1px solid var(--color-border);")

    lines = []
    lines.append("### Blazor Implementation")
    lines.append("")
    lines.append(
        "Apply this design system only after the host project is inspected. "
        "Blazor is an implementation stack: the palette, typography and layout "
        "rules above stay the source of truth."
    )
    lines.append("")

    lines.append("#### 1. Identify the Blazor application model")
    lines.append("")
    lines.append("| Project signal | Model | Design rules |")
    lines.append("|----------------|-------|--------------|")
    lines.append(
        "| `Program.cs` with `AddRazorComponents` / `MapRazorComponents` | "
        "Blazor Web App (.NET 8+) | Static SSR by default; Interactive Server, "
        "WebAssembly or Auto per component/page. |"
    )
    lines.append(
        "| `wwwroot/index.html` + `WebAssemblyHostBuilder` | "
        "Standalone Blazor WebAssembly | Client rendering only; budget the "
        "download, show a first-load state, no circuits. |"
    )
    lines.append(
        "| `Startup.cs` + `_Host.cshtml` + `MapBlazorHub` | "
        "Legacy Blazor Server (pre-.NET 8) | Keep circuit/reconnection UX; do "
        "not introduce Web App render modes. |"
    )
    lines.append(
        "| `BlazorWebView` in a MAUI project | .NET MAUI Blazor Hybrid | "
        "No render modes; design for touch, safe areas, keyboard and offline. |"
    )
    lines.append("")
    lines.append(
        "Never apply Blazor Web App render-mode guidance to MAUI Blazor Hybrid, "
        "and never add a render mode to a legacy Blazor Server project during a "
        "visual refresh."
    )
    lines.append("")

    lines.append("#### 2. Design tokens")
    lines.append("")
    lines.append(
        "Write the `--tokens css` output to `wwwroot/css/tokens.css` and link it "
        "exactly once, before `app.css`:"
    )
    lines.append("")
    lines.append("```html")
    lines.append("<!-- App.razor (Blazor Web App) or wwwroot/index.html (standalone WASM / MAUI Hybrid) -->")
    lines.append('<link rel="stylesheet" href="css/tokens.css" />')
    lines.append("```")
    lines.append("")
    lines.append(
        "Component styles consume variables instead of repeating hex values, "
        "which keeps CSS isolation, the component-library theme and RTL support "
        "in step:"
    )
    lines.append("")
    lines.append("```css")
    lines.append("/* Dashboard.razor.css */")
    lines.append(".dashboard {")
    lines.append("    display: grid;")
    lines.append(f"    gap: {spacing_md};")
    lines.append(f"    padding-inline: {spacing_lg};")
    lines.append(f"    color: {foreground_ref};")
    lines.append(f"    background: {background_ref};")
    lines.append("}")
    lines.append("")
    lines.append(".dashboard__card {")
    for declaration in card_declarations:
        lines.append(f"    {declaration}")
    lines.append(f"    border-inline-start: 3px solid {primary_ref};")
    lines.append("    border-radius: 12px;")
    lines.append("}")
    lines.append("```")
    lines.append("")
    lines.append(
        "`--tokens css` emits the palette roles, the font stacks and, only when a "
        "density dial is set, `--space-*` steps. Radii are not exported: keep "
        "them as values here or define `--radius-*` in `app.css`."
    )
    lines.append("")
    lines.append(
        "Use `padding-inline` / `margin-inline` / `border-inline-start` rather "
        "than left/right so the same components mirror for Arabic, Hebrew and "
        "Persian. Set `dir` and `lang` on `<html>` once."
    )
    lines.append("")

    lines.append("#### 3. Typography in the Blazor shell")
    lines.append("")
    lines.append(
        f"These fonts belong in the host document (`App.razor` head or "
        f"`wwwroot/index.html`): **{heading}** for headings, **{body}** for body "
        "text. Keep them as tokens (`--font-heading`, `--font-body`) so "
        "component CSS never names a family directly."
    )
    lines.append("")

    lines.append("#### 4. Theme the installed component library")
    lines.append("")
    lines.append(
        "Read the `.csproj` `PackageReference` entries first. Prefer the "
        "installed library, do not add a second one, and do not invent "
        "parameters for a version you have not checked."
    )
    lines.append("")
    palette_class = "PaletteDark" if dark_system else "PaletteLight"
    palette_comment = (
        "    // Dark system: this palette is what MudThemeProvider.IsDarkMode renders. "
        "Only add a PaletteLight if you also ship a light theme."
        if dark_system
        else "    // Light system: add a PaletteDark from dark-mode tokens before exposing the toggle."
    )
    lines.append(
        "- **MudBlazor** — map this palette into the `MudTheme` palette that matches "
        "the generated mode instead of overriding `.mud-*` CSS:"
    )
    lines.append("")
    lines.append("```csharp")
    lines.append("var theme = new MudTheme")
    lines.append("{")
    lines.append(f"    {palette_class} = new {palette_class}")
    lines.append("    {")
    lines.append(f'        Primary = "{primary}",')
    lines.append(f'        Secondary = "{secondary}",')
    lines.append(f'        Background = "{background}",')
    lines.append(f'        Surface = "{surface}",')
    lines.append(f'        TextPrimary = "{foreground}",')
    lines.append(f'        TextSecondary = "{muted_foreground}",')
    lines.append(f'        Error = "{destructive}",')
    lines.append("    },")
    lines.append(palette_comment)
    lines.append("};")
    lines.append("```")
    lines.append("")
    lines.append(
        "- **Radzen Blazor** — register `RadzenTheme` in the layout and map the "
        "same values onto its CSS variables (`--rz-primary`, `--rz-secondary`), "
        "verifying names against the installed version. Use `RadzenDataGrid` "
        "server-side paging/filtering instead of a hand-rolled table."
    )
    lines.append(
        f"- **Microsoft Fluent UI Blazor** — `<FluentDesignTheme CustomColor=\"{primary}\" />` "
        "and consume its tokens in custom components."
    )
    lines.append(
        "- **Syncfusion / Telerik / DevExpress** — use the vendor theme package "
        "matching the installed version and the generated accent color; never "
        "edit vendor CSS."
    )
    lines.append(
        "- **Blazorise** — register a provider with one design system "
        "(`AddBlazorise(...).AddBootstrap5Providers()` or equivalent) before "
        "using its components."
    )
    lines.append(
        "- **No library installed** — native HTML elements styled by these "
        "tokens are the correct answer. A new dependency is not a design fix."
    )
    lines.append("")

    lines.append("#### 5. Interaction states from this palette")
    lines.append("")
    lines.append(
        f"- Primary action: `{primary}` with accessible on-primary text; hover "
        f"uses `color-mix(in srgb, {primary_ref} 88%, black)`."
    )
    lines.append(
        f"- Secondary/neutral action: `{secondary}` on `{surface}` with a "
        f"`{border}` border."
    )
    lines.append(
        f"- Accent/selection: `{accent}` used sparingly for the active row or "
        "selected tab."
    )
    lines.append(
        f"- Destructive action: `{destructive}` plus a confirmation dialog and a "
        "text label; never color alone."
    )
    lines.append(
        f"- Focus: a visible 2px `{focus_ref}` ring with 2px offset on "
        "every interactive element."
    )
    lines.append("")

    lines.append("#### 6. Verification checklist")
    lines.append("")
    lines.append(f"- [ ] Layouts checked at {breakpoints}")
    lines.append("- [ ] Tokens linked once; no hard-coded hex values in `.razor` or `.razor.css`")
    lines.append("- [ ] Render mode chosen per interaction boundary and prerender handled")
    lines.append("- [ ] Keyboard focus visible, dialogs trap and restore focus")
    lines.append("- [ ] Async results announced with `aria-live`/`role=\"status\"`")
    lines.append("- [ ] RTL verified with `dir=\"rtl\"` and logical properties")
    lines.append("- [ ] Dark mode uses the dark palette/tokens, not inverted colors")
    lines.append("- [ ] Component library theme applied through its own API")
    lines.append("- [ ] `dotnet build` (and existing tests) run after the change")
    lines.append("")

    return "\n".join(lines)
