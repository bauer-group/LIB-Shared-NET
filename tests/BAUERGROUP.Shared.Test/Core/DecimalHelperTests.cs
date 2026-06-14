using BAUERGROUP.Shared.Core.Extensions;

namespace BAUERGROUP.Shared.Test.Core;

public class DecimalHelperTests
{
    [Fact]
    public void FormatNumber_WithPoint_UsesInvariantCulture()
    {
        var value = 1234.56m;
        var result = value.FormatNumber(DecimalNumberFormat.Point);

        result.Should().Be("1234.56");
    }

    [Fact]
    public void FormatNumber_WithComma_UsesGermanCulture()
    {
        var value = 1234.56m;
        var result = value.FormatNumber(DecimalNumberFormat.Comma);

        result.Should().Contain(",");
    }

    [Fact]
    public void FormatNumber_Zero_ReturnsZero()
    {
        var value = 0m;
        var result = value.FormatNumber(DecimalNumberFormat.Point);

        result.Should().Be("0");
    }

    [Fact]
    public void FormatNumber_NegativeValue_PreservesSign()
    {
        var value = -123.45m;
        var result = value.FormatNumber(DecimalNumberFormat.Point);

        result.Should().StartWith("-");
    }

    [Fact]
    public void FormatNumber_LargeValue_FormatsCorrectly()
    {
        var value = 1234567.89m;
        var result = value.FormatNumber(DecimalNumberFormat.Point);

        result.Should().Contain("1234567.89");
    }
}
