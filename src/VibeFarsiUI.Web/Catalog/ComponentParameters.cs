using System.Reflection;
using Microsoft.AspNetCore.Components;

namespace VibeFarsiUI.Web.Catalog;

/// <summary>Builds parameter tables straight from the <c>[Parameter]</c> attributes.</summary>
public static class ComponentParameters
{
    public sealed record Row(string Name, string Type, string Default, string Description);

    private static readonly Dictionary<string, string> Descriptions = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ChildContent"] = "محتوای داخل کامپوننت",
        ["Class"] = "کلاس‌های CSS اضافه",
        ["Variant"] = "نوع ظاهری",
        ["Size"] = "اندازه",
        ["Value"] = "مقدار قابل اتصال با @bind",
        ["ValueChanged"] = "رخداد تغییر مقدار",
        ["Checked"] = "وضعیت انتخاب",
        ["CheckedChanged"] = "رخداد تغییر وضعیت",
        ["Disabled"] = "غیرفعال‌سازی",
        ["Label"] = "برچسب",
        ["Hint"] = "متن راهنما",
        ["Error"] = "پیام خطا",
        ["Placeholder"] = "متن جای‌نگهدار",
        ["Id"] = "شناسه یکتای عنصر",
        ["Options"] = "فهرست گزینه‌ها",
        ["Items"] = "فهرست آیتم‌ها",
        ["Active"] = "تب فعال",
        ["ActiveChanged"] = "رخداد تغییر تب",
        ["Page"] = "شماره صفحه",
        ["PageChanged"] = "رخداد تغییر صفحه",
        ["PageCount"] = "تعداد صفحه‌ها",
        ["Text"] = "متن",
        ["Title"] = "عنوان",
        ["Description"] = "توضیح",
        ["Amount"] = "مبلغ",
        ["OldAmount"] = "مبلغ پیش از تخفیف",
        ["ShowValue"] = "نمایش مقدار",
        ["Max"] = "حداکثر",
        ["Min"] = "حداقل",
        ["ShowToday"] = "نمایش دکمه امروز",
        ["Initials"] = "حروف اول نام",
        ["Src"] = "نشانی تصویر",
        ["Alt"] = "متن جایگزین تصویر",
        ["Caption"] = "عنوان جدول",
        ["OnComplete"] = "رخداد تکمیل کد",
        ["Automatically"] = "خودکار"
    };

    public static IReadOnlyList<Row> For(Type type)
    {
        var instance = TryCreate(type);
        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => (Property: p, Attribute: p.GetCustomAttribute<ParameterAttribute>()))
            .Where(x => x.Attribute is not null)
            .OrderBy(x => x.Attribute!.CaptureUnmatchedValues ? 1 : 0)
            .ThenBy(x => x.Property.Name, StringComparer.Ordinal)
            .Select(x => new Row(
                x.Property.Name,
                PrettyType(x.Property.PropertyType),
                Default(instance, x.Property),
                Descriptions.GetValueOrDefault(x.Property.Name, "—")))
            .ToList();
    }

    private static object? TryCreate(Type type)
    {
        try { return Activator.CreateInstance(type); }
        catch { return null; }
    }

    private static string Default(object? instance, PropertyInfo property)
    {
        if (instance is null) return "—";
        try
        {
            var value = property.GetValue(instance);
            return value switch
            {
                null => "—",
                string s when s.Length == 0 => "\"\"",
                string s => $"\"{s}\"",
                bool b => b ? "true" : "false",
                EventCallback => "—",
                Microsoft.AspNetCore.Components.RenderFragment => "—",
                _ when property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(EventCallback<>) => "—",
                _ => value.ToString() ?? "—"
            };
        }
        catch
        {
            return "—";
        }
    }

    private static string PrettyType(Type type)
    {
        if (type == typeof(string)) return "string";
        if (type == typeof(bool)) return "bool";
        if (type == typeof(int)) return "int";
        if (type == typeof(double)) return "double";
        if (type == typeof(DateTime)) return "DateTime";
        if (type == typeof(DateTime?)) return "DateTime?";
        if (type == typeof(RenderFragment)) return "RenderFragment";
        if (type.IsGenericType)
        {
            var name = type.Name[..type.Name.IndexOf('`')];
            var args = string.Join(", ", type.GetGenericArguments().Select(PrettyType));
            return $"{name}<{args}>";
        }
        return type.Name;
    }
}
