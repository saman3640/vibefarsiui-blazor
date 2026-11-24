using System.Text;
using VibeFarsiUI.Core.Registry;
using VibeFarsiUI.Infrastructure;
using VibeFarsiUI.Infrastructure.Registry;
using VibeFarsiUI.Web.Components;
using VibeFarsiUI.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddSingleton<RazorSourceCatalog>();

builder.Services.AddVibeFarsiInfrastructure(options =>
{
    options.CatalogPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "registry", "catalog.json");
    options.DocumentsPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "registry", "documents.json");
    options.ContentRoot = Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "registry", "content");
    options.BaseUrl = builder.Configuration["Site:Url"] ?? "https://vibefarsi.ir";
});

var app = builder.Build();
Console.WriteLine(app.Environment.EnvironmentName);
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

static string Singular(string type) => type switch
{
    "components" => "component",
    "charts" => "chart",
    "calendars" => "calendar",
    "animations" => "animation",
    "backgrounds" => "background",
    "templates" => "template",
    "blocks" => "block",
    "sites" => "site",
    "themes" => "theme",
    "skills" => "skill",
    _ => type
};

// Machine-readable registry, contract-compatible with the Node CLI and MCP server.
app.MapGet("/r", (IRegistryProvider registry) => Results.Json(registry.Catalog));
app.MapGet("/r/catalog", (IRegistryProvider registry) => Results.Json(registry.Catalog));
app.MapGet("/r/{type}/{slug}", (string type, string slug, IRegistryProvider registry) =>
{
    var clean = slug.EndsWith(".json", StringComparison.OrdinalIgnoreCase) ? slug[..^5] : slug;
    var document = registry.GetDocument(Singular(type), clean);
    return document is null ? Results.NotFound(new { error = "not_found" }) : Results.Json(document);
});

// SEO endpoints.
app.MapGet("/robots.txt", (IRegistryProvider registry, RegistryOptions options) =>
{
    var baseUrl = options.BaseUrl.TrimEnd('/');
    var text = $"User-agent: *\nAllow: /\nSitemap: {baseUrl}/sitemap.xml\n";
    return Results.Text(text, "text/plain", Encoding.UTF8);
});

app.MapGet("/sitemap.xml", (IRegistryProvider registry, RegistryOptions options) =>
{
    var baseUrl = options.BaseUrl.TrimEnd('/');
    var paths = new List<string>
    {
        "/", "/components", "/blocks", "/charts", "/calendar", "/animations",
        "/backgrounds", "/templates", "/themes", "/skills", "/docs", "/icons"
    };
    paths.AddRange(registry.Items
        .Where(i => i.Type is not "lib" and not "theme" and not "skill")
        .Select(i => $"/{i.SectionPath}/{i.Slug}"));
    paths.AddRange(registry.Items
        .Where(i => i.Type is "theme" or "skill")
        .Select(i => $"/{i.SectionPath}/{i.Slug}"));

    var sb = new StringBuilder();
    sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n");
    sb.Append("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">\n");
    foreach (var path in paths.Distinct())
        sb.Append($"  <url><loc>{baseUrl}{path}</loc></url>\n");
    sb.Append("</urlset>\n");
    return Results.Text(sb.ToString(), "application/xml", Encoding.UTF8);
});

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
