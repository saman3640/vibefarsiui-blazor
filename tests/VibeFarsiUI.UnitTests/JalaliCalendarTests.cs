using VibeFarsiUI.Core.Calendar;

namespace VibeFarsiUI.UnitTests;

public class JalaliCalendarTests
{
    [Fact]
    public void Converts_gregorian_to_jalali()
    {
        var j = JalaliCalendar.ToJalali(new DateTime(2026, 9, 16));
        Assert.Equal(new JalaliDate(1405, 6, 25), j);
    }

    [Fact]
    public void Converts_jalali_to_gregorian()
    {
        Assert.Equal(new DateTime(2026, 9, 16), JalaliCalendar.ToGregorian(1405, 6, 25));
        Assert.Equal(new DateTime(2026, 3, 21), JalaliCalendar.ToGregorian(1405, 1, 1));
    }

    [Fact]
    public void Round_trips_a_full_year()
    {
        for (var day = new DateTime(2026, 1, 1); day < new DateTime(2027, 1, 1); day = day.AddDays(1))
        {
            var j = JalaliCalendar.ToJalali(day);
            Assert.Equal(day, JalaliCalendar.ToGregorian(j.Year, j.Month, j.Day));
        }
    }

    [Theory]
    [InlineData(1403, true)]
    [InlineData(1404, false)]
    [InlineData(1405, false)]
    public void Leap_years_match_the_official_calendar(int year, bool leap)
    {
        Assert.Equal(leap, JalaliCalendar.IsLeapYear(year));
    }

    [Fact]
    public void Month_lengths_follow_the_jalali_rule()
    {
        Assert.Equal(31, JalaliCalendar.MonthLength(1405, 1));
        Assert.Equal(31, JalaliCalendar.MonthLength(1405, 6));
        Assert.Equal(30, JalaliCalendar.MonthLength(1405, 7));
        Assert.Equal(30, JalaliCalendar.MonthLength(1405, 11));
        Assert.Equal(30, JalaliCalendar.MonthLength(1403, 12));
        Assert.Equal(29, JalaliCalendar.MonthLength(1404, 12));
    }

    [Fact]
    public void Week_starts_on_saturday()
    {
        // 2026-09-19 is a Saturday.
        Assert.Equal(0, JalaliCalendar.Weekday(new DateTime(2026, 9, 19)));
        Assert.Equal("شنبه", JalaliCalendar.WeekdayNames[0]);
    }

    [Fact]
    public void Formats_persian_dates()
    {
        var date = new DateTime(2026, 9, 16);
        Assert.Equal("چهارشنبه، ۲۵ شهریور ۱۴۰۵", JalaliCalendar.Format(date, weekday: true));
        Assert.Equal("۲۵ شهریور ۱۴۰۵", JalaliCalendar.Format(date));
        Assert.Equal("۱۴۰۵/۰۶/۲۵", JalaliCalendar.FormatNumeric(date));
    }

    [Fact]
    public void Calendar_math_builds_saturday_first_month_grids()
    {
        var grid = CalendarMath.MonthGrid(1405, 6);
        Assert.Equal(5, grid.Length);
        Assert.Equal(DayOfWeek.Saturday, grid[0][0].DayOfWeek);
        Assert.Equal(25, JalaliCalendar.ToJalali(new DateTime(2026, 9, 16)).Day);
        Assert.Equal("شهریور ۱۴۰۵", CalendarMath.MonthTitle(1405, 6));
        Assert.Equal("امروز", CalendarMath.DayLabel(new DateTime(2026, 9, 16), new DateTime(2026, 9, 16)));
    }

    [Fact]
    public void Add_jalali_months_clamps_day_31()
    {
        var start = JalaliCalendar.ToGregorian(1405, 6, 31);
        var next = CalendarMath.AddJalaliMonths(start, 1);
        Assert.Equal(new JalaliDate(1405, 7, 30), JalaliCalendar.ToJalali(next));
    }
}
