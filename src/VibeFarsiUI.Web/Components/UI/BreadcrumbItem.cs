namespace VibeFarsiUI.Web.Components.UI;

/// <summary>One crumb for <see cref="Breadcrumb"/>. A null <see cref="Href"/> renders as current page.</summary>
public sealed record BreadcrumbItem(string Label, string? Href = null);
