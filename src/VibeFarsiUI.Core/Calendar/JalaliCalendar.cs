using System.Globalization;
using VibeFarsiUI.Core.Text;

namespace VibeFarsiUI.Core.Calendar;

/// <summary>A Jalali (شمسی) calendar date.</summary>
public readonly record struct JalaliDate(int Year, int Month, int Day) : IComparable<JalaliDate>
{
    public int CompareTo(JalaliDate other)
    {
        var y = Year.CompareTo(other.Year);
        if (y != 0) return y;
        var m = Month.CompareTo(other.Month);
        return m != 0 ? m : Day.CompareTo(other.Day);
    }

    public override string ToString() =>
        $"{Year:0000}/{Month:00}/{Day:00}";

    public static bool operator <(JalaliDate a, JalaliDate b) => a.CompareTo(b) < 0;
    public static bool operator >(JalaliDate a, JalaliDate b) => a.CompareTo(b) > 0;
    public static bool operator <=(JalaliDate a, JalaliDate b) => a.CompareTo(b) <= 0;
    public static bool operator >=(JalaliDate a, JalaliDate b) => a.CompareTo(b) >= 0;
}

/// <summary>
/// Jalali conversion using the Borkowski algorithm as used by jalaali-js
/// (ported 1:1 from <c>lib/jalali.ts</c>). Valid for Jalali years 1…3177.
/// </summary>
public static class JalaliCalendar
{
    public static readonly string[] MonthNames =
        ["فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند"];

    public static readonly string[] WeekdayNames =
        ["شنبه", "یکشنبه", "دوشنبه", "سه‌شنبه", "چهارشنبه", "پنجشنبه", "جمعه"];

    public static readonly string[] WeekdayShortNames = ["ش", "ی", "د", "س", "چ", "پ", "ج"];

    private static readonly int[] Breaks =
    [
        -61, 9, 38, 199, 426, 686, 756, 818, 1111, 1181, 1210,
        1635, 2060, 2097, 2192, 2262, 2324, 2394, 2456, 3178
    ];

    private static int Div(int a, int b) => (int)Math.Truncate((double)a / b);
    private static int Mod(int a, int b) => a - Div(a, b) * b;

    private readonly record struct JalCalResult(int Leap, int Gy, int March);

    private static JalCalResult JalCal(int jy)
    {
        var bl = Breaks.Length;
        var gy = jy + 621;
        var leapJ = -14;
        var jp = Breaks[0];
        if (jy < jp || jy >= Breaks[bl - 1])
            throw new ArgumentOutOfRangeException(nameof(jy), $"Invalid Jalali year {jy}.");

        var jump = 0;
        for (var i = 1; i < bl; i++)
        {
            var jm = Breaks[i];
            jump = jm - jp;
            if (jy < jm) break;
            leapJ = leapJ + Div(jump, 33) * 8 + Div(Mod(jump, 33), 4);
            jp = jm;
        }
        var n = jy - jp;
        leapJ = leapJ + Div(n, 33) * 8 + Div(Mod(n, 33) + 3, 4);
        if (Mod(jump, 33) == 4 && jump - n == 4) leapJ++;
        var leapG = Div(gy, 4) - Div((Div(gy, 100) + 1) * 3, 4) - 150;
        var march = 20 + leapJ - leapG;
        if (jump - n < 6) n = n - jump + Div(jump + 4, 33) * 33;
        var leap = Mod(Mod(n + 1, 33) - 1, 4);
        if (leap == -1) leap = 4;
        return new JalCalResult(leap, gy, march);
    }

    private static int G2D(int gy, int gm, int gd)
    {
        var d = Div((gy + Div(gm - 8, 6) + 100100) * 1461, 4)
                + Div(153 * Mod(gm + 9, 12) + 2, 5)
                + gd - 34840408;
        d = d - Div(Div(gy + 100100 + Div(gm - 8, 6), 100) * 3, 4) + 752;
        return d;
    }

    private static (int Gy, int Gm, int Gd) D2G(int jdn)
    {
        var j = 4 * jdn + 139361631;
        j = j + Div(Div(4 * jdn + 183187720, 146097) * 3, 4) * 4 - 3908;
        var i = Div(Mod(j, 1461), 4) * 5 + 308;
        var gd = Div(Mod(i, 153), 5) + 1;
        var gm = Mod(Div(i, 153), 12) + 1;
        var gy = Div(j, 1461) - 100100 + Div(8 - gm, 6);
        return (gy, gm, gd);
    }

    private static int J2D(int jy, int jm, int jd)
    {
        var r = JalCal(jy);
        return G2D(r.Gy, 3, r.March) + (jm - 1) * 31 - Div(jm, 7) * (jm - 7) + jd - 1;
    }

    private static JalaliDate D2J(int jdn)
    {
        var gy = D2G(jdn).Gy;
        var jy = gy - 621;
        var r = JalCal(jy);
        var jdn1f = G2D(gy, 3, r.March);
        var k = jdn - jdn1f;
        if (k >= 0)
        {
            if (k <= 185) return new JalaliDate(jy, 1 + Div(k, 31), Mod(k, 31) + 1);
            k -= 186;
        }
        else
        {
            jy--;
            k += 179;
            if (r.Leap == 1) k++;
        }
        return new JalaliDate(jy, 7 + Div(k, 30), Mod(k, 30) + 1);
    }

    public static JalaliDate ToJalali(DateTime date)
        => D2J(G2D(date.Year, date.Month, date.Day));

    public static JalaliDate ToJalali(DateOnly date)
        => D2J(G2D(date.Year, date.Month, date.Day));

    public static DateTime ToGregorian(int jy, int jm, int jd)
    {
        var (gy, gm, gd) = D2G(J2D(jy, jm, jd));
        return new DateTime(gy, gm, gd, 0, 0, 0, DateTimeKind.Unspecified);
    }

    public static DateOnly ToGregorianDateOnly(JalaliDate date) => DateOnly.FromDateTime(ToGregorian(date.Year, date.Month, date.Day));

    public static bool IsLeapYear(int jy) => JalCal(jy).Leap == 0;

    public static int MonthLength(int jy, int jm)
    {
        if (jm is < 1 or > 12) throw new ArgumentOutOfRangeException(nameof(jm));
        if (jm <= 6) return 31;
        if (jm <= 11) return 30;
        return IsLeapYear(jy) ? 30 : 29;
    }

    /// <summary>0 = شنبه … 6 = جمعه.</summary>
    public static int Weekday(DateTime date) => (int)(((int)date.DayOfWeek + 1) % 7);

    public static int Weekday(JalaliDate date) => Weekday(ToGregorian(date.Year, date.Month, date.Day));

    /// <summary>«چهارشنبه، ۲۵ شهریور ۱۴۰۵».</summary>
    public static string Format(DateTime date, bool weekday = false, bool year = true)
    {
        var (jy, jm, jd) = ToJalali(date);
        var text = $"{PersianText.Fa(jd)} {MonthNames[jm - 1]}";
        if (year) text += $" {PersianText.Fa(jy)}";
        return (weekday ? $"{WeekdayNames[Weekday(date)]}، " : string.Empty) + text;
    }

    public static string Format(JalaliDate date, bool weekday = false, bool year = true)
        => Format(ToGregorian(date.Year, date.Month, date.Day), weekday, year);

    /// <summary>Numeric form «۱۴۰۵/۰۶/۲۵».</summary>
    public static string FormatNumeric(DateTime date)
    {
        var (jy, jm, jd) = ToJalali(date);
        return PersianText.Fa($"{jy}/{jm:00}/{jd:00}");
    }

    public static string FormatNumeric(JalaliDate date)
        => PersianText.Fa($"{date.Year}/{date.Month:00}/{date.Day:00}");
}
