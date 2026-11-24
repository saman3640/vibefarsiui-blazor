using Bunit;
using Microsoft.AspNetCore.Components;
using VibeFarsiUI.Core.Text;
using VibeFarsiUI.Web.Components.Shared;
using VibeFarsiUI.Web.Components.UI;

namespace VibeFarsiUI.ComponentTests;

public sealed class NativeComponentTests : BunitContext
{
    [Fact]
    public void Button_Renders_Variant_And_Text()
    {
        var cut = Render<Button>(parameters => parameters
            .Add(p => p.Variant, "outline")
            .AddChildContent("ذخیره"));

        var button = cut.Find("button");
        Assert.Contains("vf-btn--outline", button.ClassList);
        Assert.Equal("ذخیره", button.TextContent.Trim());
        Assert.Null(button.GetAttribute("disabled"));
    }

    [Fact]
    public void Button_Disabled_Renders_Disabled_Attribute()
    {
        var cut = Render<Button>(parameters => parameters.Add(p => p.Disabled, true));
        Assert.NotNull(cut.Find("button").GetAttribute("disabled"));
    }

    [Fact]
    public void Price_Uses_Persian_Digits_And_Toman()
    {
        var cut = Render<Price>(parameters => parameters.Add(p => p.Amount, 2_450_000d));
        Assert.Contains(PersianText.FaNumber(2_450_000), cut.Markup);
        Assert.Contains("تومان", cut.Markup);
    }

    [Fact]
    public void JalaliDatePicker_Renders_Shahrivar_1405()
    {
        var cut = Render<JalaliDatePicker>(parameters => parameters
            .Add(p => p.Value, new DateTime(2026, 9, 16)));

        Assert.Contains("شهریور", cut.Markup);
        Assert.Contains(PersianText.Fa(1405), cut.Markup);
        Assert.Contains(PersianText.Fa(25), cut.Markup);
        Assert.Equal(7, cut.FindAll(".vf-cal__weekday").Count);
        Assert.NotEmpty(cut.FindAll(".vf-cal__day[data-selected=\"true\"]"));
    }

    [Fact]
    public async Task JalaliDatePicker_Keeps_Navigated_Month_On_Rerender()
    {
        var selected = new DateTime(2026, 9, 16);
        var cut = Render<JalaliDatePicker>(parameters => parameters.Add(p => p.Value, selected));

        cut.FindAll(".vf-cal__nav")[1].Click();
        Assert.Contains("مهر", cut.Markup);

        // Simulate the parent re-rendering with the same bound value.
        await cut.InvokeAsync(() => cut.Instance.SetParametersAsync(ParameterView.FromDictionary(new Dictionary<string, object?>
        {
            [nameof(JalaliDatePicker.Value)] = selected,
            [nameof(JalaliDatePicker.Class)] = "rerender"
        })));

        Assert.Contains("مهر", cut.Markup);
    }

    [Fact]
    public void PhoneInput_Shows_Iranian_Operator()
    {
        var cut = Render<PhoneInput>(parameters => parameters.Add(p => p.Value, "09123456789"));
        Assert.Contains("همراه اول", cut.Markup);
        Assert.Contains("۰۹۱۲", cut.Markup);
    }

    [Fact]
    public void ComponentPreview_Button_Renders_Live_Component()
    {
        var cut = Render<ComponentPreview>(parameters => parameters.Add(p => p.Slug, "button"));
        Assert.Contains("ذخیره", cut.Markup);
        Assert.NotEmpty(cut.FindAll(".vf-btn"));
    }

    [Fact]
    public void Alert_Warning_Has_Role_Status_And_Title()
    {
        var cut = Render<Alert>(parameters => parameters
            .Add(p => p.Variant, "warning")
            .Add(p => p.Title, "توجه")
            .AddChildContent("متن هشدار"));

        var alert = cut.Find(".vf-alert");
        Assert.Contains("vf-alert--warning", alert.ClassList);
        Assert.Equal("status", alert.GetAttribute("role"));
        Assert.Contains("توجه", cut.Markup);
    }

    [Fact]
    public void Checkbox_Toggles_With_Click()
    {
        var changed = false;
        var cut = Render<Checkbox>(parameters => parameters
            .Add(p => p.Label, "قوانین")
            .Add(p => p.CheckedChanged, value => changed = value));

        cut.Find("input[type=checkbox]").Change(true);
        Assert.True(changed);
    }
}
