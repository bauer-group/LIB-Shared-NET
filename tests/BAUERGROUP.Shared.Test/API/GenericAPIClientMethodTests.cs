using BAUERGROUP.Shared.API.API;

namespace BAUERGROUP.Shared.Test.API;

public class GenericAPIClientMethodTests
{
    [Fact]
    public void Enum_HasGetValue()
    {
        Enum.IsDefined(typeof(GenericAPIClientMethod), GenericAPIClientMethod.GET).Should().BeTrue();
    }

    [Fact]
    public void Enum_HasPostValue()
    {
        Enum.IsDefined(typeof(GenericAPIClientMethod), GenericAPIClientMethod.POST).Should().BeTrue();
    }

    [Fact]
    public void Enum_HasPutValue()
    {
        Enum.IsDefined(typeof(GenericAPIClientMethod), GenericAPIClientMethod.PUT).Should().BeTrue();
    }

    [Fact]
    public void Enum_HasDeleteValue()
    {
        Enum.IsDefined(typeof(GenericAPIClientMethod), GenericAPIClientMethod.DELETE).Should().BeTrue();
    }

    [Fact]
    public void Enum_HasFourValues()
    {
        var values = Enum.GetValues(typeof(GenericAPIClientMethod));
        values.Length.Should().Be(4);
    }

    [Fact]
    public void Enum_CanBeConvertedToString()
    {
        GenericAPIClientMethod.GET.ToString().Should().Be("GET");
        GenericAPIClientMethod.POST.ToString().Should().Be("POST");
        GenericAPIClientMethod.PUT.ToString().Should().Be("PUT");
        GenericAPIClientMethod.DELETE.ToString().Should().Be("DELETE");
    }

    [Fact]
    public void Enum_CanBeParsedFromString()
    {
        var parsed = Enum.Parse<GenericAPIClientMethod>("GET");
        parsed.Should().Be(GenericAPIClientMethod.GET);
    }

    [Theory]
    [InlineData(GenericAPIClientMethod.GET, 0)]
    [InlineData(GenericAPIClientMethod.POST, 1)]
    [InlineData(GenericAPIClientMethod.PUT, 2)]
    [InlineData(GenericAPIClientMethod.DELETE, 3)]
    public void Enum_HasExpectedIntValues(GenericAPIClientMethod method, int expectedValue)
    {
        ((int)method).Should().Be(expectedValue);
    }
}
