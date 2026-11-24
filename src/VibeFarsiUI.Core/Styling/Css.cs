namespace VibeFarsiUI.Core.Styling;

/// <summary>
/// Small class-name joiner used instead of the React <c>cn()</c> helper.
/// Keeps component markup readable and avoids string concatenation bugs.
/// </summary>
public static class Css
{
    public static string Join(params string?[] values)
        => string.Join(' ', values.Where(v => !string.IsNullOrWhiteSpace(v)).Select(v => v!.Trim()));

    public static string When(bool condition, string? className) => condition ? className ?? string.Empty : string.Empty;

    public static string Variant(string prefix, string? value)
        => string.IsNullOrWhiteSpace(value) ? string.Empty : $"{prefix}-{value}";
}

/// <summary>Fluent builder for conditional class lists in Razor markup.</summary>
public sealed class CssBuilder
{
    private readonly List<string> _classes = [];

    public CssBuilder Add(string? className)
    {
        if (!string.IsNullOrWhiteSpace(className)) _classes.Add(className.Trim());
        return this;
    }

    public CssBuilder Add(string? className, bool condition)
    {
        if (condition) Add(className);
        return this;
    }

    public CssBuilder Add(string prefix, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value)) Add($"{prefix}-{value}");
        return this;
    }

    public override string ToString() => string.Join(' ', _classes);
}
