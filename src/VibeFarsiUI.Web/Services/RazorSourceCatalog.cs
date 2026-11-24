using System.Text.Json;

namespace VibeFarsiUI.Web.Services;

/// <summary>Loads the exported Razor sources used by the code viewer.</summary>
public sealed class RazorSourceCatalog
{
    private readonly Dictionary<string, string> _sources;

    public RazorSourceCatalog(IWebHostEnvironment env, ILogger<RazorSourceCatalog> logger)
    {
        var path = Path.Combine(env.ContentRootPath, "wwwroot", "registry", "razor-source.json");
        if (!File.Exists(path))
        {
            logger.LogWarning("Razor source export not found at {Path}. Run scripts/export-razor-source.mjs.", path);
            _sources = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            return;
        }

        var json = File.ReadAllText(path);
        _sources = JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                   ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    public string? Get(string? key)
        => key is not null && _sources.TryGetValue(key, out var source) ? source : null;
}
