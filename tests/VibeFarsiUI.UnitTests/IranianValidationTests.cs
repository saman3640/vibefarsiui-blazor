using VibeFarsiUI.Core.Iranian;

namespace VibeFarsiUI.UnitTests;

public class IranianValidationTests
{
    [Theory]
    [InlineData("۰۹۱۲۳۴۵۶۷۸۹", "9123456789")]
    [InlineData("+989123456789", "9123456789")]
    [InlineData("00989123456789", "9123456789")]
    public void NormalizeMobile_accepts_persian_and_international_forms(string input, string expected)
    {
        Assert.Equal(expected, IranianValidation.NormalizeMobile(input));
        Assert.True(IranianValidation.IsMobile(input));
    }

    [Fact]
    public void Mobile_formatting_and_operator()
    {
        Assert.Equal("912 345 6789", IranianValidation.FormatMobile("09123456789"));
        Assert.Equal("همراه اول", IranianValidation.MobileOperator("09123456789"));
        Assert.Null(IranianValidation.MobileOperator("08123456789"));
    }

    [Fact]
    public void Iban_validation_mod97()
    {
        Assert.True(IranianValidation.IsIban("IR820540102680020817909002"));
        Assert.True(IranianValidation.IsIban("IR062960000000100324200001"));
        Assert.False(IranianValidation.IsIban("IR120170000000102345678901"));
        Assert.Equal("IR820540102680020817909002", IranianValidation.NormalizeIban("ir82 0540 1026 8002 0817 9090 02"));
        Assert.Equal("IR82 0540 1026 8002 0817 9090 02", IranianValidation.FormatIban("IR820540102680020817909002"));
        Assert.Equal("پارسیان", IranianValidation.IbanBank("IR820540102680020817909002"));
    }

    [Fact]
    public void National_id_checksum()
    {
        Assert.True(IranianValidation.IsNationalId("0499370899"));
        Assert.False(IranianValidation.IsNationalId("0012345678"));
        Assert.Equal("۰۴۹-۹۳۷۰۸۹-۹", IranianValidation.FormatNationalId("0499370899"));
    }

    [Fact]
    public void Card_luhn_and_bank_bin()
    {
        Assert.True(IranianValidation.IsCardNumber("6037991122334455"));
        Assert.False(IranianValidation.IsCardNumber("6037997591234567"));
        Assert.Equal("۶۰۳۷ ۹۹۱۱ ۲۲۳۳ ۴۴۵۵", IranianValidation.FormatCardNumber("6037991122334455"));
        Assert.Equal("ملی", IranianValidation.CardBank("6037991122334455"));
    }

    [Fact]
    public void Plate_parse_format_and_validate()
    {
        var plate = IranianValidation.ParsePlate("۱۲ ب ۳۴۵ ایران ۱۱");
        Assert.Equal(new PlateValue("12", "ب", "345", "11"), plate);
        Assert.True(IranianValidation.IsPlate(plate));
        Assert.Equal("۱۲ ب ۳۴۵ ایران ۱۱", IranianValidation.FormatPlate(plate));
        Assert.Equal("12ب345-11", IranianValidation.StringifyPlate(plate));
    }

    [Fact]
    public void Postal_code_formatting()
    {
        Assert.Equal("۸۱۶۳۸-۴۷۳۵۱", IranianValidation.FormatPostalCode("8163847351"));
        Assert.True(IranianValidation.IsPostalCode("8163847351"));
        Assert.False(IranianValidation.IsPostalCode("0000000000"));
    }

    [Fact]
    public void Legal_id_and_bill_helpers_exist()
    {
        Assert.Equal(11, IranianValidation.NormalizeLegalId("۱۰۱۰۱۰۱۰۱۰۱").Length);
        Assert.False(IranianValidation.IsLegalId("00000000000"));
        Assert.Equal("برق", IranianValidation.BillType("1234567890123"));
        Assert.Equal(0, IranianValidation.BillAmountRial("12"));
    }
}
