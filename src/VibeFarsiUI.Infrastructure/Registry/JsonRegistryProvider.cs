using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VibeFarsiUI.Core.Registry;
using VibeFarsiUI.Core.Text;

namespace VibeFarsiUI.Infrastructure.Registry;

/// <summary>
/// Loads the exported registry catalog and per-item documents once at startup.
/// The JSON is generated from the original TypeScript metadata by
/// <c>scripts/export-registry.mjs</c>, so the <c>/r</c> contract stays intact.
/// </summary>
public sealed class JsonRegistryProvider : IRegistryProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly RegistryOptions _options;
    private readonly ILogger<JsonRegistryProvider> _logger;
    private readonly Lazy<RegistryCatalog> _catalog;
    private readonly Lazy<Dictionary<string, RegistryDocument>> _documents;
    private readonly Dictionary<string, RegistryItemSummary> _index = new(StringComparer.OrdinalIgnoreCase);

    public JsonRegistryProvider(IOptions<RegistryOptions> options, ILogger<JsonRegistryProvider> logger)
    {
        _options = options.Value;
        _logger = logger;
        _catalog = new Lazy<RegistryCatalog>(LoadCatalog);
        _documents = new Lazy<Dictionary<string, RegistryDocument>>(LoadDocuments);
    }

    public RegistryCatalog Catalog
    {
        get
        {
            var catalog = _catalog.Value;
            EnsureIndex(catalog);
            return catalog;
        }
    }

    public IReadOnlyList<RegistryItemSummary> Items => Catalog.Items;

    public IReadOnlyList<CategoryInfo> ComponentCategories => CategoriesFor("component");

    private static readonly Dictionary<string, string> ComponentCategoryLabels = new()
    {
        ["form"] = "فرم",
        ["display"] = "نمایش داده",
        ["feedback"] = "بازخورد",
        ["nav"] = "ناوبری",
        ["overlay"] = "پنجره و منو",
        ["data"] = "داده و نمودار",
        ["education"] = "آموزشی",
        ["ai"] = "هوش مصنوعی"
    };

    private static readonly Dictionary<string, string> ChartCategoryLabels = new()
    {
        ["kit"] = "کیت مشترک",
        ["trend"] = "روند",
        ["compare"] = "مقایسه",
        ["part"] = "سهم از کل",
        ["distribution"] = "پراکندگی",
        ["flow"] = "جریان",
        ["finance"] = "مالی"
    };

    private static readonly Dictionary<string, string> CalendarCategoryLabels = new()
    {
        ["kit"] = "کیت مشترک",
        ["views"] = "نماها",
        ["occasions"] = "مناسبت‌ها",
        ["booking"] = "نوبت‌دهی",
        ["planning"] = "برنامه‌ریزی"
    };

    public IReadOnlyList<CategoryInfo> CategoriesFor(string type)
    {
        var labels = type switch
        {
            "component" => ComponentCategoryLabels,
            "chart" => ChartCategoryLabels,
            "calendar" => CalendarCategoryLabels,
            _ => new Dictionary<string, string>()
        };

        var counts = Items
            .Where(i => string.Equals(i.Type, type, StringComparison.OrdinalIgnoreCase))
            .Where(i => !string.IsNullOrWhiteSpace(i.Category))
            .GroupBy(i => i.Category!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

        var result = new List<CategoryInfo>
        {
            new() { Key = "all", Label = "همه", Count = counts.Values.Sum() }
        };
        result.AddRange(counts.OrderByDescending(c => c.Value).Select(c => new CategoryInfo
        {
            Key = c.Key,
            Label = labels.GetValueOrDefault(c.Key, c.Key),
            Count = c.Value
        }));
        return result;
    }

    public RegistryItemSummary? Find(string type, string slug)
    {
        var catalog = Catalog;
        EnsureIndex(catalog);
        return _index.GetValueOrDefault($"{type}/{slug}");
    }

    public RegistryItemSummary? FindBySlug(string slug)
        => Items.FirstOrDefault(i => string.Equals(i.Slug, slug, StringComparison.OrdinalIgnoreCase));

    public IReadOnlyList<RegistryItemSummary> BySection(string section)
        => Items.Where(i => string.Equals(i.SectionPath, section, StringComparison.OrdinalIgnoreCase)).ToList();

    public IReadOnlyList<RegistryItemSummary> Search(string? query, string? type = null, int limit = 50)
    {
        var items = Items.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(type))
            items = items.Where(i => string.Equals(i.Type, type, StringComparison.OrdinalIgnoreCase));

        var normalized = PersianText.NormalizeForSearch(query);
        if (string.IsNullOrWhiteSpace(normalized))
            return items.Take(limit).ToList();

        var scored = new List<(RegistryItemSummary Item, int Score)>();
        foreach (var item in items)
        {
            var score = Score(item, normalized);
            if (score > 0) scored.Add((item, score));
        }
        return scored
            .OrderByDescending(s => s.Score)
            .ThenBy(s => s.Item.Name, StringComparer.Ordinal)
            .Take(limit)
            .Select(s => s.Item)
            .ToList();
    }

    private static int Score(RegistryItemSummary item, string query)
    {
        var score = 0;
        var slug = PersianText.NormalizeForSearch(item.Slug);
        var name = PersianText.NormalizeForSearch(item.Name);
        var desc = PersianText.NormalizeForSearch(item.Desc);
        var aliases = item.Aliases.Select(PersianText.NormalizeForSearch).ToList();
        var tags = item.Tags.Select(PersianText.NormalizeForSearch).ToList();

        if (slug == query) score += 120;
        else if (name == query) score += 100;
        else if (aliases.Contains(query)) score += 90;

        if (score == 0)
        {
            if (slug.StartsWith(query, StringComparison.Ordinal)) score += 70;
            else if (name.StartsWith(query, StringComparison.Ordinal)) score += 60;
            else if (slug.Contains(query, StringComparison.Ordinal)) score += 50;
            else if (name.Contains(query, StringComparison.Ordinal)) score += 45;
            else if (aliases.Any(a => a.Contains(query, StringComparison.Ordinal))) score += 38;
            else if (tags.Any(t => t.Contains(query, StringComparison.Ordinal))) score += 24;
            else if (desc.Contains(query, StringComparison.Ordinal)) score += 18;
        }
        return score;
    }

    public RegistryDocument? GetDocument(string type, string slug)
    {
        var key = $"{DocumentSection(type)}/{slug}";
        return _documents.Value.GetValueOrDefault(key);
    }

    public string? GetSource(string type, string slug)
        => GetDocument(type, slug)?.Files.FirstOrDefault()?.Content;

    /// <summary>URL collection name used by the legacy <c>/r</c> routes.</summary>
    public static string DocumentSection(string type) => type switch
    {
        "component" => "components",
        "chart" => "charts",
        "calendar" => "calendars",
        "animation" => "animations",
        "background" => "backgrounds",
        "template" => "templates",
        "block" => "blocks",
        "site" => "sites",
        "theme" => "themes",
        "skill" => "skills",
        "lib" => "lib",
        _ => type
    };

    private void EnsureIndex(RegistryCatalog catalog)
    {
        if (_index.Count == catalog.Items.Count && _index.Count > 0) return;
        lock (_index)
        {
            if (_index.Count == catalog.Items.Count && _index.Count > 0) return;
            _index.Clear();
            foreach (var item in catalog.Items)
                _index[$"{item.Type}/{item.Slug}"] = item;
        }
    }

    private string ResolvePath(string configured)
        => Path.IsPathRooted(configured) ? configured : Path.Combine(AppContext.BaseDirectory, configured);

    private RegistryCatalog LoadCatalog()
    {
        var path = ResolvePath(_options.CatalogPath);
        if (!File.Exists(path))
            throw new FileNotFoundException($"Registry catalog not found at {path}. Run scripts/export-registry.mjs.", path);

        _logger.LogInformation("Loading registry catalog from {Path}", path);
        var json = File.ReadAllText(path);
        var catalog = JsonSerializer.Deserialize<RegistryCatalog>(json, JsonOptions)
                      ?? throw new InvalidOperationException("Registry catalog could not be deserialized.");
        _logger.LogInformation("Loaded {Count} registry items", catalog.Items.Count);
        return catalog;
    }

    private Dictionary<string, RegistryDocument> LoadDocuments()
    {
        var path = ResolvePath(_options.DocumentsPath);
        if (!File.Exists(path))
        {
            _logger.LogWarning("Registry documents not found at {Path}; per-item documents will be empty", path);
            return new Dictionary<string, RegistryDocument>(StringComparer.OrdinalIgnoreCase);
        }

        var json = File.ReadAllText(path);
        var docs = JsonSerializer.Deserialize<Dictionary<string, RegistryDocument>>(json, JsonOptions)
                   ?? new Dictionary<string, RegistryDocument>();
        return new Dictionary<string, RegistryDocument>(docs, StringComparer.OrdinalIgnoreCase);
    }
}
