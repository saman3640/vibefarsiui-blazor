using System.Text.Json.Serialization;

namespace VibeFarsiUI.Core.Registry;

/// <summary>
/// One entry of the machine-readable catalog. Mirrors the TypeScript
/// <c>CatalogItem</c> shape served by <c>/r/catalog</c>.
/// </summary>
public sealed class RegistryItemSummary
{
    [JsonPropertyName("type")] public string Type { get; set; } = string.Empty;
    [JsonPropertyName("slug")] public string Slug { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("nameEn")] public string? NameEn { get; set; }
    [JsonPropertyName("desc")] public string Desc { get; set; } = string.Empty;
    [JsonPropertyName("file")] public string File { get; set; } = string.Empty;
    [JsonPropertyName("target")] public string Target { get; set; } = string.Empty;
    [JsonPropertyName("url")] public string Url { get; set; } = string.Empty;
    [JsonPropertyName("deps")] public List<string> Deps { get; set; } = [];
    [JsonPropertyName("registryDeps")] public List<string> RegistryDeps { get; set; } = [];
    [JsonPropertyName("tags")] public List<string> Tags { get; set; } = [];
    [JsonPropertyName("category")] public string? Category { get; set; }
    [JsonPropertyName("aliases")] public List<string> Aliases { get; set; } = [];
    [JsonPropertyName("prompt")] public string? Prompt { get; set; }

    [JsonIgnore] public string PersianType => Type switch
    {
        "component" => "کامپوننت",
        "chart" => "نمودار",
        "calendar" => "تقویم",
        "animation" => "انیمیشن",
        "background" => "پس‌زمینه",
        "template" => "قالب",
        "block" => "بلاک",
        "site" => "سایت",
        "theme" => "تم",
        "lib" => "کتابخانه",
        "skill" => "مهارت",
        _ => Type
    };

    [JsonIgnore] public string SectionPath => Type switch
    {
        "component" => "components",
        "chart" => "charts",
        "calendar" => "calendar",
        "animation" => "animations",
        "background" => "backgrounds",
        "template" => "templates",
        "block" => "blocks",
        "site" => "sites",
        "theme" => "themes",
        "skill" => "skills",
        _ => "docs"
    };
}

public sealed class RegistryCatalog
{
    [JsonPropertyName("name")] public string Name { get; set; } = "vibefarsi";
    [JsonPropertyName("homepage")] public string Homepage { get; set; } = "https://vibefarsi.ir";
    [JsonPropertyName("schema")] public string Schema { get; set; } = "https://vibefarsi.dev/schema/registry-item.json";
    [JsonPropertyName("items")] public List<RegistryItemSummary> Items { get; set; } = [];
}

/// <summary>Per-item JSON, contract-compatible with <c>/r/{type}/{slug}.json</c>.</summary>
public sealed class RegistryDocument
{
    [JsonPropertyName("$schema")] public string Schema { get; set; } = "https://vibefarsi.dev/schema/registry-item.json";
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("type")] public string Type { get; set; } = string.Empty;
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("description")] public string Description { get; set; } = string.Empty;
    [JsonPropertyName("dependencies")] public List<string> Dependencies { get; set; } = [];
    [JsonPropertyName("registryDependencies")] public List<string> RegistryDependencies { get; set; } = [];
    [JsonPropertyName("files")] public List<RegistryFile> Files { get; set; } = [];
    [JsonPropertyName("css")] public string? Css { get; set; }
    [JsonPropertyName("prompt")] public string? Prompt { get; set; }
}

public sealed class RegistryFile
{
    [JsonPropertyName("path")] public string Path { get; set; } = string.Empty;
    [JsonPropertyName("content")] public string Content { get; set; } = string.Empty;
    [JsonPropertyName("type")] public string Type { get; set; } = "registry:component";
}

public sealed class CategoryInfo
{
    public string Key { get; init; } = "all";
    public string Label { get; init; } = "همه";
    public int Count { get; init; }
}

/// <summary>Read side of the registry, implemented by Infrastructure.</summary>
public interface IRegistryProvider
{
    RegistryCatalog Catalog { get; }
    IReadOnlyList<RegistryItemSummary> Items { get; }
    IReadOnlyList<CategoryInfo> ComponentCategories { get; }
    IReadOnlyList<CategoryInfo> CategoriesFor(string type);
    RegistryItemSummary? Find(string type, string slug);
    RegistryItemSummary? FindBySlug(string slug);
    IReadOnlyList<RegistryItemSummary> BySection(string section);
    IReadOnlyList<RegistryItemSummary> Search(string? query, string? type = null, int limit = 50);
    RegistryDocument? GetDocument(string type, string slug);
    string? GetSource(string type, string slug);
}
