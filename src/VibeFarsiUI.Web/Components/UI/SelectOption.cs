namespace VibeFarsiUI.Web.Components.UI;

/// <summary>One option for <see cref="Select"/> and <see cref="RadioGroup"/>.</summary>
public sealed record SelectOption(string Value, string Label, bool Disabled = false);

/// <summary>One radio card option.</summary>
public sealed record RadioOption(string Value, string Label, string? Description = null, bool Disabled = false);
