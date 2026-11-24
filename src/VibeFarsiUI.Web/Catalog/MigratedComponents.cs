using System.Reflection;
using VibeFarsiUI.Web.Components.UI;

namespace VibeFarsiUI.Web.Catalog;

/// <summary>
/// Maps registry slugs to their native Blazor implementation. Only components
/// listed here render a live preview; the rest are shown with an explicit
/// migration notice rather than a fake mock-up.
/// </summary>
public static class MigratedComponents
{
    public sealed record Entry(string Slug, Type ComponentType, string SourceKey, string Usage);

    private static readonly Dictionary<string, Entry> Entries = new(StringComparer.OrdinalIgnoreCase)
    {
        ["button"] = new("button", typeof(Button), "Button", "<Button Variant=\"default\">ذخیره</Button>"),
        ["input"] = new("input", typeof(Input), "Input", "<Field Label=\"نام\">\n    <Input @bind-Value=\"name\" Placeholder=\"نام خود را بنویسید\" />\n</Field>"),
        ["textarea"] = new("textarea", typeof(Textarea), "Textarea", "<Textarea @bind-Value=\"note\" Label=\"یادداشت\" ShowCount MaxLength=\"200\" />"),
        ["select"] = new("select", typeof(Select), "Select", "<Select @bind-Value=\"city\" Label=\"شهر\" Options=\"Options\" />"),
        ["otp-field"] = new("otp-field", typeof(OtpField), "OtpField", "<OtpField @bind-Value=\"code\" OnComplete=\"VerifyAsync\" />"),
        ["number-field"] = new("number-field", typeof(NumberField), "NumberField", "<NumberField @bind-Value=\"count\" Min=\"1\" Max=\"10\" Label=\"تعداد\" />"),
        ["checkbox-group"] = new("checkbox-group", typeof(Checkbox), "Checkbox", "<Checkbox @bind-Checked=\"accepted\" Label=\"قوانین را می‌پذیرم\" />"),
        ["radio-group"] = new("radio-group", typeof(RadioGroup), "RadioGroup", "<RadioGroup @bind-Value=\"plan\" Options=\"Plans\" />"),
        ["switch"] = new("switch", typeof(Switch), "Switch", "<Switch @bind-Checked=\"enabled\" Label=\"اعلان‌ها\" />"),
        ["rating"] = new("rating", typeof(Rating), "Rating", "<Rating @bind-Value=\"score\" ShowValue />"),
        ["tooltip"] = new("tooltip", typeof(Tooltip), "Tooltip", "<Tooltip Text=\"کپی در حافظه\">\n    <Button Variant=\"outline\" Size=\"sm\">نگه دارید</Button>\n</Tooltip>"),
        ["tabs"] = new("tabs", typeof(Tabs), "Tabs", "<Tabs Items=\"Tabs\" @bind-Active=\"active\" />"),
        ["pagination"] = new("pagination", typeof(Pagination), "Pagination", "<Pagination @bind-Page=\"page\" PageCount=\"8\" />"),
        ["breadcrumb"] = new("breadcrumb", typeof(Breadcrumb), "Breadcrumb", "<Breadcrumb Items=\"Crumbs\" />"),
        ["alert"] = new("alert", typeof(Alert), "Alert", "<Alert Variant=\"success\" Title=\"ذخیره شد\">تغییرات شما ثبت شد.</Alert>"),
        ["progress"] = new("progress", typeof(Progress), "Progress", "<Progress Value=\"72\" Label=\"پیشرفت\" ShowValue />"),
        ["skeleton"] = new("skeleton", typeof(Skeleton), "Skeleton", "<Skeleton Height=\"1rem\" Width=\"60%\" />"),
        ["empty-state"] = new("empty-state", typeof(EmptyState), "EmptyState", "<EmptyState Title=\"نتیجه‌ای نیست\" Description=\"فیلترها را تغییر دهید.\" />"),
        ["badge"] = new("badge", typeof(Badge), "Badge", "<Badge Variant=\"brand\" Solid>جدید</Badge>"),
        ["avatar"] = new("avatar", typeof(Avatar), "Avatar", "<Avatar Initials=\"س‌ا\" Alt=\"سارا احمدی\" />"),
        ["table"] = new("table", typeof(Table), "Table", "<Table Caption=\"گزارش\">\n    <thead>…</thead>\n    <tbody>…</tbody>\n</Table>"),
        ["stat"] = new("stat", typeof(Stat), "Stat", "<Stat Label=\"فروش امروز\" Value=\"۲٬۴۵۰٬۰۰۰ تومان\" Hint=\"+۱۲٪ نسبت به دیروز\" />"),
        ["price"] = new("price", typeof(Price), "Price", "<Price Amount=\"2450000\" OldAmount=\"2900000\" Size=\"lg\" />"),
        ["accordion"] = new("accordion", typeof(Accordion), "Accordion", "<Accordion>\n    <AccordionItem Title=\"ارسال\">…</AccordionItem>\n</Accordion>"),
        ["kbd"] = new("kbd", typeof(Kbd), "Kbd", "<Kbd>Ctrl</Kbd> + <Kbd>K</Kbd>"),
        ["card"] = new("card", typeof(Card), "Card", "<Card Title=\"عنوان\" Description=\"توضیح کوتاه\">محتوا</Card>"),
        ["phone-input"] = new("phone-input", typeof(PhoneInput), "PhoneInput", "<PhoneInput @bind-Value=\"phone\" />"),
        ["national-id-input"] = new("national-id-input", typeof(NationalIdInput), "NationalIdInput", "<NationalIdInput @bind-Value=\"nationalId\" />"),
        ["amount-input"] = new("amount-input", typeof(AmountInput), "AmountInput", "<AmountInput @bind-Value=\"amount\" Label=\"مبلغ\" />"),
        ["search-input"] = new("search-input", typeof(SearchInput), "SearchInput", "<SearchInput @bind-Value=\"query\" Placeholder=\"جست‌وجوی محصول…\" />"),
        ["separator"] = new("separator", typeof(Separator), "Separator", "<Separator Label=\"یا\" />"),
        ["spinner"] = new("spinner", typeof(Spinner), "Spinner", "<Spinner Label=\"در حال دریافت\" />"),
        ["relative-time"] = new("relative-time", typeof(RelativeTime), "RelativeTime", "<RelativeTime Value=\"DateTime.Now.AddHours(-3)\" />"),
        ["copy-button"] = new("copy-button", typeof(CopyButton), "CopyButton", "<CopyButton Text=\"dotnet add package VibeFarsiUI\" />"),
        ["date-picker"] = new("date-picker", typeof(JalaliDatePicker), "JalaliDatePicker", "<JalaliDatePicker @bind-Value=\"date\" />"),
        ["calendar"] = new("calendar", typeof(JalaliDatePicker), "JalaliDatePicker", "<JalaliDatePicker @bind-Value=\"date\" />")
    };

    public static bool TryGet(string? slug, out Entry entry)
    {
        if (slug is not null && Entries.TryGetValue(slug, out var found)) { entry = found; return true; }
        entry = null!;
        return false;
    }

    public static Entry? Get(string? slug) => TryGet(slug, out var entry) ? entry : null;

    /// <summary>Number of distinct native Razor component types.</summary>
    public static int Count => Entries.Values.Select(e => e.ComponentType).Distinct().Count();

    /// <summary>Number of registry slugs that have a live preview.</summary>
    public static int SlugCount => Entries.Count;

    public static IReadOnlyCollection<string> Slugs => Entries.Keys;
}
