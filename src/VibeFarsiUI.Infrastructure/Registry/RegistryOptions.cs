namespace VibeFarsiUI.Infrastructure.Registry;

/// <summary>Configuration for the JSON-backed registry.</summary>
public sealed class RegistryOptions
{
    public const string SectionName = "Registry";

    /// <summary>Absolute or content-root-relative path to catalog.json.</summary>
    public string CatalogPath { get; set; } = "wwwroot/registry/catalog.json";

    /// <summary>Absolute or content-root-relative path to documents.json.</summary>
    public string DocumentsPath { get; set; } = "wwwroot/registry/documents.json";

    /// <summary>Root of the copied TypeScript sources (used for fallback reads).</summary>
    public string ContentRoot { get; set; } = "wwwroot/registry/content";

    /// <summary>Public base URL used for absolute links.</summary>
    public string BaseUrl { get; set; } = "https://vibefarsi.ir";
}
