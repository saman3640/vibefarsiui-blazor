using VibeFarsiUI.Core.Formatting;
using VibeFarsiUI.Core.Text;

namespace VibeFarsiUI.UnitTests;

public class PersianTextTests
{
    [Fact]
    public void Fa_converts_latin_digits()
    {
        Assert.Equal("۱۴۰۵", PersianText.Fa("1405"));
        Assert.Equal("سال ۱۴۰۵", PersianText.Fa("سال 1405"));
    }

    [Fact]
    public void En_converts_persian_and_arabic_digits()
    {
        Assert.Equal("1405", PersianText.En("۱۴۰۵"));
        Assert.Equal("1405", PersianText.En("١٤٠٥"));
    }

    [Fact]
    public void FaNumber_uses_persian_thousands_separator()
    {
        Assert.Equal("۱۲٬۴۵۰٬۰۰۰", PersianText.FaNumber(12_450_000));
        Assert.Equal("۱٬۲۵۰ تومان", PersianText.FormatToman(1250));
    }

    [Theory]
    [InlineData(512, "۵۱۲ بایت")]
    [InlineData(2048, "۲ کیلوبایت")]
    public void FaFileSize_formats_units(long bytes, string expected)
    {
        Assert.Equal(expected, PersianText.FaFileSize(bytes));
    }

    [Fact]
    public void NormalizeForSearch_folds_arabic_and_spaces()
    {
        Assert.Equal("علیاباد", PersianText.NormalizeForSearch("علي اباد"));
        Assert.Equal("کدملی", PersianText.NormalizeForSearch("کد‌ملی"));
        Assert.Equal(PersianText.NormalizeForSearch("تهران"), PersianText.NormalizeForSearch("  تهران "));
    }

    [Fact]
    public void NumberToWords_handles_scales_and_fractions()
    {
        Assert.Equal("یک میلیون و دویست و پنجاه هزار", PersianNumberWords.NumberToWords(1_250_000));
        Assert.Equal("صفر", PersianNumberWords.NumberToWords(0));
        Assert.Equal("منفی چهل و دو", PersianNumberWords.NumberToWords(-42));
        Assert.Equal("دوازده ممیز پنج", PersianNumberWords.NumberToWords(12.5m));
        Assert.Equal("۱۲٬۵۰۰ تومان", PersianText.FormatToman(12_500));
    }

    [Fact]
    public void AmountToWords_appends_unit()
    {
        Assert.Equal("یک میلیون و دویست و پنجاه هزار تومان", PersianNumberWords.TomanToWords(1_250_000));
        Assert.Equal("پانصد ریال", PersianNumberWords.RialToWords(500));
    }
}
