using System.Globalization;
using System.Text;
using VibeFarsiUI.Core.Text;

namespace VibeFarsiUI.Core.Formatting;

/// <summary>Persian number-to-words, ported from <c>lib/number-to-words.ts</c>.</summary>
public static class PersianNumberWords
{
    private static readonly string[] Ones = ["", "یک", "دو", "سه", "چهار", "پنج", "شش", "هفت", "هشت", "نه"];
    private static readonly string[] Teens = ["ده", "یازده", "دوازده", "سیزده", "چهارده", "پانزده", "شانزده", "هفده", "هجده", "نوزده"];
    private static readonly string[] Tens = ["", "", "بیست", "سی", "چهل", "پنجاه", "شصت", "هفتاد", "هشتاد", "نود"];
    private static readonly string[] Hundreds = ["", "صد", "دویست", "سیصد", "چهارصد", "پانصد", "ششصد", "هفتصد", "هشتصد", "نهصد"];
    private static readonly string[] Scales = ["", "هزار", "میلیون", "میلیارد", "تریلیون", "کوادریلیون", "کوینتیلیون", "سکستیلیون"];
    private const string And = " و ";

    private static string Group(int n)
    {
        var parts = new List<string>();
        var h = n / 100;
        var rest = n % 100;
        if (h > 0) parts.Add(Hundreds[h]);
        if (rest is >= 10 and < 20) parts.Add(Teens[rest - 10]);
        else
        {
            var t = rest / 10;
            var o = rest % 10;
            if (t > 0) parts.Add(Tens[t]);
            if (o > 0) parts.Add(Ones[o]);
        }
        return string.Join(And, parts);
    }

    private sealed record Parsed(bool Negative, string Integer, string Fraction);

    private static Parsed? Parse(string input)
    {
        var s = PersianText.En(input)
            .Replace("٬", string.Empty)
            .Replace(",", string.Empty)
            .Replace("_", string.Empty)
            .Replace(" ", string.Empty)
            .Replace('٫', '.')
            .Trim();
        if (s.Length == 0) return null;

        var negative = s.StartsWith('-');
        if (s.StartsWith('+') || s.StartsWith('-')) s = s[1..];

        var dot = s.IndexOf('.');
        var intPart = dot >= 0 ? s[..dot] : s;
        var fracPart = dot >= 0 ? s[(dot + 1)..] : string.Empty;
        if (intPart.Length == 0 && fracPart.Length == 0) return null;
        foreach (var ch in intPart) if (!char.IsDigit(ch)) return null;
        foreach (var ch in fracPart) if (!char.IsDigit(ch)) return null;

        intPart = intPart.Length == 0 ? "0" : intPart.TrimStart('0');
        if (intPart.Length == 0) intPart = "0";
        fracPart = fracPart.TrimEnd('0');
        return new Parsed(negative, intPart, fracPart);
    }

    private static string IntegerWords(string digits)
    {
        if (digits.All(c => c == '0')) return "صفر";
        var groups = new List<string>();
        for (var end = digits.Length; end > 0; end -= 3)
        {
            var start = Math.Max(0, end - 3);
            groups.Insert(0, digits[start..end]);
        }
        if (groups.Count > Scales.Length)
            throw new ArgumentOutOfRangeException(nameof(digits), "Number too large for PersianNumberWords.");

        var parts = new List<string>();
        for (var i = 0; i < groups.Count; i++)
        {
            var n = int.Parse(groups[i], CultureInfo.InvariantCulture);
            if (n == 0) continue;
            var scale = Scales[groups.Count - 1 - i];
            if (scale == "هزار" && n == 1) parts.Add("هزار");
            else parts.Add(scale.Length == 0 ? Group(n) : $"{Group(n)} {scale}");
        }
        return string.Join(And, parts);
    }

    /// <summary>1250000 → «یک میلیون و دویست و پنجاه هزار».</summary>
    public static string NumberToWords(long value) => NumberToWords(value.ToString(CultureInfo.InvariantCulture));

    public static string NumberToWords(decimal value)
        => NumberToWords(value.ToString(CultureInfo.InvariantCulture));

    /// <summary>Accepts Persian or Latin digits, «٬»/«,» separators and a decimal point.</summary>
    public static string NumberToWords(string? input)
    {
        if (input is null) return string.Empty;
        var parsed = Parse(input);
        if (parsed is null) return string.Empty;

        var words = IntegerWords(parsed.Integer);
        if (parsed.Fraction.Length > 0)
        {
            var fractionWords = parsed.Fraction
                .Select(d => d == '0' ? "صفر" : Ones[d - '0']);
            words += " ممیز " + string.Join(' ', fractionWords);
        }
        return parsed.Negative && words != "صفر" ? $"منفی {words}" : words;
    }

    /// <summary>«یک میلیون و دویست و پنجاه هزار تومان»؛ fractions dropped.</summary>
    public static string AmountToWords(decimal value, string unit = "تومان")
    {
        var parsed = Parse(value.ToString(CultureInfo.InvariantCulture));
        if (parsed is null) return string.Empty;
        var words = IntegerWords(parsed.Integer);
        var prefix = parsed.Negative && words != "صفر" ? "منفی " : string.Empty;
        return $"{prefix}{words} {unit}".Trim();
    }

    public static string TomanToWords(decimal value) => AmountToWords(value, "تومان");

    public static string RialToWords(decimal value) => AmountToWords(value, "ریال");
}
