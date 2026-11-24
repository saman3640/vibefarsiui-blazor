using System.Text;
using VibeFarsiUI.Core.Text;

namespace VibeFarsiUI.Core.Iranian;

/// <summary>Iranian car plate parts (پلاک).</summary>
public readonly record struct PlateValue(string Left, string Letter, string Middle, string Region)
{
    public static readonly PlateValue Empty = new(string.Empty, string.Empty, string.Empty, string.Empty);
    public bool IsEmpty => Left.Length == 0 && Letter.Length == 0 && Middle.Length == 0 && Region.Length == 0;
}

/// <summary>
/// Iranian-specific validation and formatting ported from <c>lib/persian.ts</c>:
/// mobile numbers, شبا (IBAN), کد ملی, bank cards, car plates, postal codes,
/// legal ids, bill ids and payment ids. Persian and Latin digits are accepted.
/// </summary>
public static class IranianValidation
{
    /* ---------- mobile ---------- */

    /// <summary>Normalizes «۰۹۱۲…», «+98912…», «0098912…» to the 10-digit form «912…».</summary>
    public static string NormalizeMobile(string? input)
    {
        var digits = new string(PersianText.En(input).Where(char.IsDigit).ToArray());
        if (digits.StartsWith("0098")) digits = digits[4..];
        else if (digits.StartsWith("98") && digits.Length > 10) digits = digits[2..];
        if (digits.StartsWith('0')) digits = digits[1..];
        return digits.Length > 10 ? digits[..10] : digits;
    }

    public static bool IsMobile(string? input) => NormalizeMobile(input) is { Length: 10 } d && d[0] == '9' && d.All(char.IsDigit);

    /// <summary>«912 345 6789» — three groups, always LTR.</summary>
    public static string FormatMobile(string? input)
    {
        var d = NormalizeMobile(input);
        var parts = new[] { d[..Math.Min(3, d.Length)], Slice(d, 3, 6), Slice(d, 6, 10) };
        return string.Join(' ', parts.Where(p => p.Length > 0));
    }

    private static string Slice(string value, int start, int end)
        => start >= value.Length ? string.Empty : value[start..Math.Min(end, value.Length)];

    /// <summary>Operator from the normalized 10-digit number, matching the TS regex rules.</summary>
    public static string? MobileOperator(string? input)
    {
        var d = NormalizeMobile(input);
        if (d.Length < 3) return null;
        if (d.StartsWith("91") || (d.StartsWith("99") && d[2] <= '6')) return "همراه اول";
        if (d.StartsWith("93") || (d.StartsWith("90") && d[2] is >= '1' and <= '5')) return "ایرانسل";
        if (d.StartsWith("92") && d[2] <= '2') return "رایتل";
        if (d.StartsWith("998")) return "شاتل موبایل";
        if (d.StartsWith("999")) return "سامانتل";
        return null;
    }

    /* ---------- شبا / IBAN ---------- */

    /// <summary>Keeps «IR» + up to 24 digits, upper-cased, digits normalized.</summary>
    public static string NormalizeIban(string? input)
    {
        var raw = PersianText.En(input).ToUpperInvariant();
        var lettersAndDigits = new string(raw.Where(char.IsLetterOrDigit).ToArray());
        var withoutIr = lettersAndDigits.StartsWith("IR") ? lettersAndDigits[2..] : lettersAndDigits;
        var digits = new string(withoutIr.Where(char.IsDigit).ToArray());
        return "IR" + (digits.Length > 24 ? digits[..24] : digits);
    }

    /// <summary>Standard IBAN mod-97 check for IR accounts (IR + 24 digits).</summary>
    public static bool IsIban(string? input)
    {
        var iban = NormalizeIban(input);
        if (iban.Length != 26) return false;
        var rearranged = iban[4..] + "1827" + iban[2..4];
        var remainder = 0;
        foreach (var ch in rearranged)
        {
            if (!char.IsDigit(ch)) return false;
            remainder = (remainder * 10 + (ch - '0')) % 97;
        }
        return remainder == 1;
    }

    /// <summary>«IR12 0170 0000 0010 2345 6789 01».</summary>
    public static string FormatIban(string? input)
    {
        var iban = NormalizeIban(input);
        var sb = new StringBuilder(iban.Length + 7);
        for (var i = 0; i < iban.Length; i++)
        {
            if (i > 0 && i % 4 == 0) sb.Append(' ');
            sb.Append(iban[i]);
        }
        return sb.ToString();
    }

    private static readonly Dictionary<string, string> Banks = new()
    {
        ["010"] = "بانک مرکزی", ["011"] = "صنعت و معدن", ["012"] = "ملت", ["013"] = "رفاه کارگران",
        ["014"] = "مسکن", ["015"] = "سپه", ["016"] = "کشاورزی", ["017"] = "ملی", ["018"] = "تجارت",
        ["019"] = "صادرات", ["020"] = "توسعه صادرات", ["021"] = "پست بانک", ["022"] = "توسعه تعاون",
        ["051"] = "مؤسسه توسعه", ["053"] = "کارآفرین", ["054"] = "پارسیان", ["055"] = "اقتصاد نوین",
        ["056"] = "سامان", ["057"] = "پاسارگاد", ["058"] = "سرمایه", ["059"] = "سینا",
        ["060"] = "قرض‌الحسنه مهر ایران", ["061"] = "شهر", ["062"] = "آینده", ["064"] = "گردشگری",
        ["066"] = "دی", ["069"] = "ایران زمین", ["070"] = "رسالت", ["078"] = "خاورمیانه",
        ["079"] = "مؤسسه ملل"
    };

    public static string? IbanBank(string? input)
    {
        var iban = NormalizeIban(input);
        return iban.Length < 7 ? null : Banks.GetValueOrDefault(iban.Substring(4, 3));
    }

    /// <summary>«ملت» → «بانک ملت»; names already carrying بانک/مؤسسه are left alone.</summary>
    public static string BankLabel(string name)
        => name.Contains("بانک") || name.Contains("مؤسسه") ? name : $"بانک {name}";

    /* ---------- کد ملی ---------- */

    public static string NormalizeNationalId(string? input)
    {
        var d = new string(PersianText.En(input).Where(char.IsDigit).ToArray());
        return d.Length > 10 ? d[..10] : d;
    }

    /// <summary>«۰۰۱-۲۳۴۵۶۷-۸» — the 3-6-1 grouping printed on the card.</summary>
    public static string FormatNationalId(string? input)
    {
        var d = NormalizeNationalId(input);
        var parts = new[] { Slice(d, 0, 3), Slice(d, 3, 9), Slice(d, 9, 10) };
        return PersianText.Fa(string.Join('-', parts.Where(p => p.Length > 0)));
    }

    public static bool IsNationalId(string? input)
    {
        var d = new string(PersianText.En(input).Where(char.IsDigit).ToArray());
        if (d.Length != 10 || d.Distinct().Count() == 1) return false;
        var check = d[9] - '0';
        var sum = 0;
        for (var i = 0; i < 9; i++) sum += (d[i] - '0') * (10 - i);
        var r = sum % 11;
        return r < 2 ? check == r : check == 11 - r;
    }

    /* ---------- card ---------- */

    public static string NormalizeCardNumber(string? input)
    {
        var d = new string(PersianText.En(input).Where(char.IsDigit).ToArray());
        return d.Length > 16 ? d[..16] : d;
    }

    /// <summary>«۶۰۳۷ ۹۹۱۱ ۲۲۳۳ ۴۴۵۵» — Persian digits, four groups.</summary>
    public static string FormatCardNumber(string? input)
    {
        var d = NormalizeCardNumber(input);
        var parts = Enumerable.Range(0, (d.Length + 3) / 4)
            .Select(i => d.Substring(i * 4, Math.Min(4, d.Length - i * 4)));
        return PersianText.Fa(string.Join(' ', parts));
    }

    private static readonly Dictionary<string, string> CardBins = new()
    {
        ["603799"] = "ملی", ["589210"] = "سپه", ["627381"] = "سپه", ["639599"] = "سپه",
        ["636949"] = "سپه", ["639370"] = "سپه", ["505801"] = "سپه", ["627648"] = "توسعه صادرات",
        ["207177"] = "توسعه صادرات", ["627961"] = "صنعت و معدن", ["603770"] = "کشاورزی",
        ["639217"] = "کشاورزی", ["628023"] = "مسکن", ["627760"] = "پست بانک", ["502908"] = "توسعه تعاون",
        ["627412"] = "اقتصاد نوین", ["622106"] = "پارسیان", ["639194"] = "پارسیان", ["627884"] = "پارسیان",
        ["502229"] = "پاسارگاد", ["639347"] = "پاسارگاد", ["627488"] = "کارآفرین", ["502910"] = "کارآفرین",
        ["621986"] = "سامان", ["639346"] = "سینا", ["639607"] = "سرمایه", ["636214"] = "آینده",
        ["502806"] = "شهر", ["504706"] = "شهر", ["502938"] = "دی", ["603769"] = "صادرات",
        ["610433"] = "ملت", ["991975"] = "ملت", ["627353"] = "تجارت", ["585983"] = "تجارت",
        ["589463"] = "رفاه کارگران", ["504172"] = "رسالت", ["606373"] = "قرض‌الحسنه مهر ایران",
        ["628157"] = "مؤسسه توسعه", ["505785"] = "ایران زمین", ["505416"] = "گردشگری",
        ["606256"] = "مؤسسه ملل", ["585947"] = "خاورمیانه", ["581874"] = "ایران‌ونزوئلا",
        ["636795"] = "بانک مرکزی", ["507677"] = "مؤسسه نور"
    };

    public static string? CardBank(string? input)
    {
        var d = NormalizeCardNumber(input);
        return d.Length < 6 ? null : CardBins.GetValueOrDefault(d[..6]);
    }

    public static bool IsCardNumber(string? input)
    {
        var d = NormalizeCardNumber(input);
        if (d.Length != 16) return false;
        var sum = 0;
        for (var i = 0; i < 16; i++)
        {
            var n = d[i] - '0';
            if (i % 2 == 0) { n *= 2; if (n > 9) n -= 9; }
            sum += n;
        }
        return sum % 10 == 0;
    }

    /* ---------- پلاک خودرو ---------- */

    public static readonly (string Letter, string Label)[] PlateLetters =
    [
        ("ب", "شخصی"), ("ج", "شخصی"), ("د", "شخصی"), ("س", "شخصی"), ("ص", "شخصی"),
        ("ط", "شخصی"), ("ق", "شخصی"), ("ل", "شخصی"), ("م", "شخصی"), ("ن", "شخصی"),
        ("و", "شخصی"), ("ه", "شخصی"), ("ی", "شخصی"),
        ("الف", "دولتی"), ("ت", "تاکسی"), ("ع", "حمل‌ونقل عمومی"), ("ک", "کشاورزی"),
        ("ژ", "جانبازان و معلولان"), ("پ", "نیروی انتظامی"), ("ث", "ارتش"),
        ("ز", "وزارت دفاع"), ("ش", "سپاه"), ("ف", "ستاد کل نیروهای مسلح"),
        ("گ", "گذر موقت"), ("D", "دیپلماتیک"), ("S", "کارکنان سفارت")
    ];

    private static readonly HashSet<string> PlateLetterSet = PlateLetters.Select(l => l.Letter).ToHashSet();

    /// <summary>Maps typed input to a plate letter; «ا» becomes «الف», d/s become D/S.</summary>
    public static string? PlateLetterFromKey(string? key)
    {
        if (string.IsNullOrEmpty(key)) return null;
        var k = PersianText.NormalizeArabic(key);
        if (k == "ا") k = "الف";
        if (PlateLetterSet.Contains(k)) return k;
        var upper = k.ToUpperInvariant();
        return PlateLetterSet.Contains(upper) ? upper : null;
    }

    public static bool IsPlate(PlateValue value)
        => value.Left.Length == 2 && value.Left.All(char.IsDigit)
           && PlateLetterSet.Contains(value.Letter)
           && value.Middle.Length == 3 && value.Middle.All(char.IsDigit)
           && value.Region.Length == 2 && value.Region.All(char.IsDigit);

    /// <summary>Canonical storage form «12ب345-11» (Latin digits).</summary>
    public static string StringifyPlate(PlateValue value)
        => $"{value.Left}{value.Letter}{value.Middle}{(value.Region.Length > 0 ? "-" + value.Region : string.Empty)}";

    /// <summary>Parses «12ب345-11», «۱۲ ب ۳۴۵ ایران ۱۱» or «12 الف 345 11».</summary>
    public static PlateValue ParsePlate(string? input)
    {
        if (string.IsNullOrEmpty(input)) return PlateValue.Empty;
        var s = PersianText.En(input)
            .Replace("ایران", " ")
            .Replace(" ", string.Empty)
            .Replace("-", string.Empty)
            .Replace("_", string.Empty)
            .Replace("/", string.Empty);
        if (s.Length == 0) return PlateValue.Empty;

        // Match 0-2 digits, a letter (or «الف»), 0-3 digits, 0-2 digits.
        var match = System.Text.RegularExpressions.Regex.Match(s, @"^(\d{0,2})(الف|[^\d]?)(\d{0,3})(\d{0,2})$");
        if (!match.Success) return PlateValue.Empty;
        var letter = match.Groups[2].Value is { Length: > 0 } raw ? PlateLetterFromKey(raw) ?? string.Empty : string.Empty;
        return new PlateValue(match.Groups[1].Value, letter, match.Groups[3].Value, match.Groups[4].Value);
    }

    /// <summary>Display form «۱۲ ب ۳۴۵ ایران ۱۱».</summary>
    public static string FormatPlate(PlateValue value)
    {
        if (value.IsEmpty) return string.Empty;
        return System.Text.RegularExpressions.Regex.Replace(
            $"{PersianText.Fa(value.Left)} {value.Letter} {PersianText.Fa(value.Middle)} ایران {PersianText.Fa(value.Region)}",
            @"\s+", " ").Trim();
    }

    /* ---------- postal code ---------- */

    public static string NormalizePostalCode(string? input)
    {
        var d = new string(PersianText.En(input).Where(char.IsDigit).ToArray());
        return d.Length > 10 ? d[..10] : d;
    }

    /// <summary>«۸۱۶۳۸-۴۷۳۵۱»: two groups of five.</summary>
    public static string FormatPostalCode(string? input)
    {
        var d = NormalizePostalCode(input);
        return PersianText.Fa(d.Length > 5 ? $"{d[..5]}-{d[5..]}" : d);
    }

    public static bool IsPostalCode(string? input)
    {
        var d = NormalizePostalCode(input);
        return d.Length == 10 && d.Distinct().Count() > 1;
    }

    /* ---------- شناسه ملی ---------- */

    public static string NormalizeLegalId(string? input)
    {
        var d = new string(PersianText.En(input).Where(char.IsDigit).ToArray());
        return d.Length > 11 ? d[..11] : d;
    }

    public static bool IsLegalId(string? input)
    {
        var d = new string(PersianText.En(input).Where(char.IsDigit).ToArray());
        if (d.Length != 11 || d.Distinct().Count() == 1) return false;
        var tenth = (d[9] - '0') + 2;
        int[] weights = [29, 27, 23, 19, 17, 29, 27, 23, 19, 17];
        var sum = 0;
        for (var i = 0; i < 10; i++) sum += ((d[i] - '0') + tenth) * weights[i];
        var r = sum % 11;
        return d[10] - '0' == (r == 10 ? 0 : r);
    }

    /* ---------- قبض ---------- */

    private static int BillCheckDigit(string digits)
    {
        var sum = 0;
        var weight = 2;
        for (var i = digits.Length - 1; i >= 0; i--)
        {
            sum += (digits[i] - '0') * weight;
            weight = weight == 7 ? 2 : weight + 1;
        }
        var r = sum % 11;
        return r < 2 ? 0 : 11 - r;
    }

    public static string NormalizeBillId(string? input)
    {
        var d = new string(PersianText.En(input).Where(char.IsDigit).ToArray());
        return d.Length > 13 ? d[..13] : d;
    }

    public static bool IsBillId(string? input)
    {
        var d = NormalizeBillId(input);
        if (d.Length < 6) return false;
        return BillCheckDigit(d[..^1]) == d[^1] - '0';
    }

    public static readonly Dictionary<string, string> BillTypes = new()
    {
        ["1"] = "آب", ["2"] = "برق", ["3"] = "گاز", ["4"] = "تلفن ثابت",
        ["5"] = "تلفن همراه", ["6"] = "عوارض شهرداری", ["8"] = "مالیات",
        ["9"] = "جریمه‌ی رانندگی"
    };

    public static string? BillType(string? billId)
    {
        var d = NormalizeBillId(billId);
        return d.Length >= 6 ? BillTypes.GetValueOrDefault(d[^2].ToString()) : null;
    }

    public static bool IsPaymentId(string? billId, string? paymentId)
    {
        var bill = NormalizeBillId(billId);
        var pay = NormalizeBillId(paymentId);
        if (pay.Length < 6 || !IsBillId(bill)) return false;
        var first = BillCheckDigit(pay[..^2]) == pay[^2] - '0';
        var second = BillCheckDigit(bill + pay[..^1]) == pay[^1] - '0';
        return first && second;
    }

    /// <summary>Bill amount in rial: the payment id without its last five digits, times 1000.</summary>
    public static long BillAmountRial(string? paymentId)
    {
        var pay = NormalizeBillId(paymentId);
        return pay.Length > 5 ? long.Parse(pay[..^5]) * 1000 : 0;
    }
}
