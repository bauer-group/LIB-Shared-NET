using BAUERGROUP.Shared.Core.Regions;

namespace BAUERGROUP.Shared.Test.Core;

public class CountryConverterTests
{
    [Fact]
    public void ISO2ToEnglishName_DE_ReturnsGermany()
    {
        var result = CountryConverter.ISO2ToEnglishName("DE");

        result.Should().Be("Germany");
    }

    [Fact]
    public void ISO2ToEnglishName_US_ReturnsUnitedStates()
    {
        var result = CountryConverter.ISO2ToEnglishName("US");

        result.Should().Be("United States");
    }

    [Fact]
    public void ISO2ToEnglishName_GB_ReturnsUnitedKingdom()
    {
        var result = CountryConverter.ISO2ToEnglishName("GB");

        result.Should().Be("United Kingdom");
    }

    [Fact]
    public void ISO2ToEnglishName_FR_ReturnsFrance()
    {
        var result = CountryConverter.ISO2ToEnglishName("FR");

        result.Should().Be("France");
    }

    [Fact]
    public void ISO2ToEnglishName_AT_ReturnsAustria()
    {
        var result = CountryConverter.ISO2ToEnglishName("AT");

        result.Should().Be("Austria");
    }

    [Fact]
    public void ISO2ToEnglishName_CH_ReturnsSwitzerland()
    {
        var result = CountryConverter.ISO2ToEnglishName("CH");

        result.Should().Be("Switzerland");
    }

    [Fact]
    public void ISO2ToLocalizedName_ReturnsNonEmptyString()
    {
        var result = CountryConverter.ISO2ToLocalizedName("DE");

        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ISO2ToEnglishName_InvalidCode_ThrowsException()
    {
        var action = () => CountryConverter.ISO2ToEnglishName("XX");

        action.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("DE", "Germany")]
    [InlineData("IT", "Italy")]
    [InlineData("ES", "Spain")]
    [InlineData("NL", "Netherlands")]
    [InlineData("BE", "Belgium")]
    [InlineData("PL", "Poland")]
    public void ISO2ToEnglishName_VariousCountries_ReturnsCorrectName(string iso2, string expected)
    {
        var result = CountryConverter.ISO2ToEnglishName(iso2);

        result.Should().Be(expected);
    }
}
