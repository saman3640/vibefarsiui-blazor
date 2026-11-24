namespace VibeFarsiUI.Web.Components.UI;

/// <summary>One tab item for <see cref="Tabs"/>.</summary>
public sealed record TabItem(string Key, string Label, string? Badge = null, bool Disabled = false);
