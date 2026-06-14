using BAUERGROUP.Shared.Core.Extensions;

namespace BAUERGROUP.Shared.Test.Core;

public class EmailHelperTests
{
    [Theory]
    [InlineData("test@example.com", true)]
    [InlineData("user.name@domain.co.uk", true)]
    [InlineData("user+tag@example.org", true)]
    [InlineData("invalid", false)]
    [InlineData("invalid@", false)]
    [InlineData("@invalid.com", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("   ", false)]
    public void IsEmailAddressValid_ReturnsExpected(string? email, bool expected)
    {
        var result = email!.IsEmailAddressValid();
        result.Should().Be(expected);
    }

    [Fact]
    public void IsEmailAddressValid_WithSpecialCharacters_ValidatesCorrectly()
    {
        "test.name+filter@sub.domain.com".IsEmailAddressValid().Should().BeTrue();
        "test@[192.168.1.1]".IsEmailAddressValid().Should().BeTrue();
    }

    [Fact]
    public void IsEmailAddressValid_WithInvalidFormat_ReturnsFalse()
    {
        "test@@domain.com".IsEmailAddressValid().Should().BeFalse();
        "test domain.com".IsEmailAddressValid().Should().BeFalse();
    }
}
