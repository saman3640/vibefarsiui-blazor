using System.Globalization;
using VibeFarsiUI.Core.Text;

namespace VibeFarsiUI.Core.Calendar;

/// <summary>
/// Calendar math ported from <c>lib/calendar-utils.ts</c>: day keys, Jalali week
/// and month grids, month arithmetic and Persian day labels. Weeks start on شنبه.
/// </summary>
public static class CalendarMath
{
    public const string TehranTimeZone = "Asia/Tehran";

    private static string Pad(int n) => n.ToString("00", CultureInfo.InvariantCulture);

    /// <summary>"2026-10-12" for the local calendar day of <paramref name="date"/>.</summary>
    public static string DayKey(DateTime date) => $"{date.Year}-{Pad(date.Month)}-{Pad(date.Day)}";

    public static string DayKey(DateOnly date) => $"{date.Year}-{Pad(date.Month)}-{Pad(date.Day)}";

    public static DateTime FromKey(string key)
    {
        var parts = key.Split('-');
        return new DateTime(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]));
    }

    public static DateTime AddDays(DateTime date, int days) => date.Date.AddDays(days);

    /// <summary>Whole calendar days from a to b (positive when b is later).</summary>
    public static int DiffDays(DateTime a, DateTime b)
        => (int)Math.Round((DateTime.SpecifyKind(b.Date, DateTimeKind.Utc) - DateTime.SpecifyKind(a.Date, DateTimeKind.Utc)).TotalDays);

    public static bool IsSameDay(DateTime? a, DateTime? b)
        => a.HasValue && b.HasValue && DayKey(a.Value) == DayKey(b.Value);

    /// <summary>The شنبه that starts the week of <paramref name="date"/>.</summary>
    public static DateTime StartOfJalaliWeek(DateTime date)
        => AddDays(date, -JalaliCalendar.Weekday(date));

    /// <summary>Seven days, شنبه to جمعه.</summary>
    public static DateTime[] WeekDates(DateTime date)
    {
        var start = StartOfJalaliWeek(date);
        return Enumerable.Range(0, 7).Select(i => AddDays(start, i)).ToArray();
    }

    /// <summary>Moves by Jalali months; day 31 clamps to the target month's last day.</summary>
    public static DateTime AddJalaliMonths(DateTime date, int months)
    {
        var j = JalaliCalendar.ToJalali(date);
        var total = j.Year * 12 + (j.Month - 1) + months;
        var year = (int)Math.Floor(total / 12.0);
        var month = ((total % 12) + 12) % 12 + 1;
        var day = Math.Min(j.Day, JalaliCalendar.MonthLength(year, month));
        return JalaliCalendar.ToGregorian(year, month, day);
    }

    /// <summary>Weeks (rows of 7 dates, شنبه first) covering a Jalali month.</summary>
    public static DateTime[][] MonthGrid(int jy, int jm, bool fixedWeeks = false)
    {
        var first = JalaliCalendar.ToGregorian(jy, jm, 1);
        var start = StartOfJalaliWeek(first);
        var used = JalaliCalendar.Weekday(first) + JalaliCalendar.MonthLength(jy, jm);
        var rows = fixedWeeks ? 6 : (int)Math.Ceiling(used / 7.0);
        var grid = new DateTime[rows][];
        for (var r = 0; r < rows; r++)
        {
            grid[r] = new DateTime[7];
            for (var c = 0; c < 7; c++) grid[r][c] = AddDays(start, r * 7 + c);
        }
        return grid;
    }

    /// <summary>«مهر ۱۴۰۵».</summary>
    public static string MonthTitle(int jy, int jm) => $"{JalaliCalendar.MonthNames[jm - 1]} {PersianText.Fa(jy)}";

    /// <summary>«۱۲ مهر».</summary>
    public static string ShortDay(DateTime date)
    {
        var j = JalaliCalendar.ToJalali(date);
        return $"{PersianText.Fa(j.Day)} {JalaliCalendar.MonthNames[j.Month - 1]}";
    }

    /// <summary>«امروز»، «فردا»، «دیروز» or «چهارشنبه ۲۳ مهر».</summary>
    public static string DayLabel(DateTime date, DateTime? today = null)
    {
        var d = DiffDays(today ?? DateTime.Today, date);
        return d switch
        {
            0 => "امروز",
            1 => "فردا",
            -1 => "دیروز",
            _ => $"{JalaliCalendar.WeekdayNames[JalaliCalendar.Weekday(date)]} {ShortDay(date)}"
        };
    }
}
