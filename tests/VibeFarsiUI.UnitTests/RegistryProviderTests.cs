using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using VibeFarsiUI.Infrastructure.Registry;

namespace VibeFarsiUI.UnitTests;

public class RegistryProviderTests
{
    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "VibeFarsiUI.sln")))
            dir = dir.Parent;
        return dir?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }

    private static JsonRegistryProvider Provider()
    {
        var root = RepoRoot();
        var options = Options.Create(new RegistryOptions
        {
            CatalogPath = Path.Combine(root, "src", "VibeFarsiUI.Web", "wwwroot", "registry", "catalog.json"),
            DocumentsPath = Path.Combine(root, "src", "VibeFarsiUI.Web", "wwwroot", "registry", "documents.json")
        });
        return new JsonRegistryProvider(options, NullLogger<JsonRegistryProvider>.Instance);
    }

    [Fact]
    public void Loads_all_registry_items()
    {
        var provider = Provider();
        Assert.Equal(392, provider.Items.Count);
        Assert.Equal(108, provider.Items.Count(i => i.Type == "component"));
        Assert.Equal(6, provider.Items.Count(i => i.Type == "theme"));
    }

    [Fact]
    public void Finds_items_and_documents()
    {
        var provider = Provider();
        var button = provider.Find("component", "button");
        Assert.NotNull(button);
        Assert.Equal("دکمه", button!.Name);
        Assert.Equal("/r/components/button.json", button.Url);

        var doc = provider.GetDocument("component", "button");
        Assert.NotNull(doc);
        Assert.Contains("export function Button", doc!.Files[0].Content);
        Assert.Equal("components/ui/button.tsx", doc.Files[0].Path);
        Assert.NotNull(doc.Prompt);
    }

    [Fact]
    public void Serves_theme_and_skill_documents()
    {
        var provider = Provider();
        var theme = provider.GetDocument("theme", "graphite");
        Assert.NotNull(theme);
        Assert.Equal("registry:style", theme!.Files[0].Type);
        Assert.Equal("app/globals.css", theme.Files[0].Path);
        Assert.Contains("--background", theme.Files[0].Content);

        var skill = provider.GetDocument("skill", "persian-rtl-ui");
        Assert.NotNull(skill);
        Assert.Equal("registry:skill", skill!.Files[0].Type);
    }

    [Fact]
    public void Search_matches_persian_aliases()
    {
        var provider = Provider();
        var results = provider.Search("تقویم", limit: 10);
        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.Slug == "calendar");

        var toman = provider.Search("تومان", limit: 10);
        Assert.Contains(toman, r => r.Slug == "price");
    }

    [Fact]
    public void Categories_have_persian_labels()
    {
        var provider = Provider();
        var categories = provider.ComponentCategories;
        Assert.Contains(categories, c => c.Key == "all");
        Assert.Contains(categories, c => c.Key == "form" && c.Label == "فرم");
    }
}
