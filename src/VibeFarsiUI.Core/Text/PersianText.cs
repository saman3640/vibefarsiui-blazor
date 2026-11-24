using System.Globalization;
using System.Text;

namespace VibeFarsiUI.Core.Text;

/// <summary>
/// Persian text helpers ported from <c>lib/utils.ts</c>: digit conversion,
/// thousands separators, Toman and percentage formatting, file sizes, and
/// search normalization (Arabic yeh/kaf folding, ZWNJ removal).
/// </summary>
public static class PersianText
{
    private const string PersianDigits = "۰۱۲۳۴۵۶۷۸۹";
    private const string ArabicIndicDigits = "٠١٢٣٤٥٦٧٨٩";
    public const char ThousandsSeparator = '٬';
    public const char DecimalSeparator = '٫';
    public const char Zwnj = '\u200C';

    /// <summary>Converts Latin digits in a string or number to Persian digits: 1405 → ۱۴۰۵.</summary>
    public static string Fa(string? value)
    {
        if (string.IsNullOrEmpty(value)) return value ?? string.Empty;
        var sb = new StringBuilder(value.Length);
        foreach (var ch in value)
            sb.Append(ch is >= '0' and <= '9' ? PersianDigits[ch - '0'] : ch);
        return sb.ToString();
    }

    public static string Fa(long value) => Fa(value.ToString(CultureInfo.InvariantCulture));

    /// <summary>Persian and Arabic-Indic digits back to Latin (for parsing input).</summary>
    public static string En(string? value)
    {
        if (string.IsNullOrEmpty(value)) return value ?? string.Empty;
        var sb = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            var faIndex = PersianDigits.IndexOf(ch);
            if (faIndex >= 0) { sb.Append((char)('0' + faIndex)); continue; }
            var arIndex = ArabicIndicDigits.IndexOf(ch);
            if (arIndex >= 0) { sb.Append((char)('0' + arIndex)); continue; }
            sb.Append(ch);
        }
        return sb.ToString();
    }

    /// <summary>Folds Arabic yeh/kaf and alef variants to their Persian forms.</summary>
    public static string NormalizeArabic(string? value)
    {
        if (string.IsNullOrEmpty(value)) return value ?? string.Empty;
        return value
            .Replace('ي', 'ی')
            .Replace('ك', 'ک')
            .Replace('أ', 'ا')
            .Replace('إ', 'ا')
            .Replace('ٱ', 'ا')
            .Replace('ة', 'ه');
    }

    /// <summary>
    /// Normalizes text for tolerant search: folds Arabic characters, removes
    /// diacritics, spaces and ZWNJ, and unifies alef/yeh. «علي اباد» → «علیاباد».
    /// </summary>
    public static string NormalizeForSearch(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        var normalized = NormalizeArabic(value.Normalize(NormalizationForm.FormD));
        var sb = new StringBuilder(normalized.Length);
        foreach (var ch in normalized)
        {
            if (char.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark) continue;
            if (ch is ' ' or '\t' or '\n' or '\r' or Zwnj or '\u200B' or '-' or '_' or '‌') continue;
            sb.Append(ch);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    /// <summary>Thousands-separated Persian number: 12450000 → ۱۲٬۴۵۰٬۰۰۰.</summary>
    public static string FaNumber(double value)
    {
        var rounded = Math.Round(value, MidpointRounding.AwayFromZero);
        var latin = rounded.ToString("N0", CultureInfo.InvariantCulture);
        return Fa(latin.Replace(',', ThousandsSeparator));
    }

    /// <summary>Amount in Toman with unit: 12450000 → ۱۲٬۴۵۰٬۰۰۰ تومان.</summary>
    public static string FormatToman(double value) => $"{FaNumber(value)} تومان";

    /// <summary>Percent with Persian digits and the Persian percent sign: 18 → ۱۸٪.</summary>
    public static string FaPercent(double value, int digits = 0)
        => $"{Fa(value.ToString("F" + digits, CultureInfo.InvariantCulture))}٪";

    /// <summary>File size in Persian: 1258291 → ۱٫۲ مگابایت.</summary>
    public static string FaFileSize(long bytes)
    {
        if (bytes < 1024) return $"{Fa(bytes)} بایت";
        if (bytes < 1024L * 1024L)
            return $"{Fa((bytes / 1024.0).ToString("F0", CultureInfo.InvariantCulture))} کیلوبایت";
        var mb = (bytes / (1024.0 * 1024.0)).ToString("F1", CultureInfo.InvariantCulture);
        return $"{Fa(mb).Replace('.', DecimalSeparator)} مگابایت";
    }

    /// <summary>Millis to a Persian «۲:۰۵» clock label.</summary>
    public static string FormatMinutes(int minutes)
    {
        var m = ((minutes % 1440) + 1440) % 1440;
        return Fa($"{m / 60:00}:{m % 60:00}");
    }

    /// <summary>«۱ ساعت و ۳۰ دقیقه»، «۴۵ دقیقه»، «۲ ساعت».</summary>
    public static string FormatDuration(int minutes)
    {
        var hours = minutes / 60;
        var rest = minutes % 60;
        if (hours == 0) return $"{Fa(rest)} دقیقه";
        return rest == 0 ? $"{Fa(hours)} ساعت" : $"{Fa(hours)} ساعت و {Fa(rest)} دقیقه";
    }
}
