using BAUERGROUP.Shared.Core.Extensions;

namespace BAUERGROUP.Shared.Test.Core;

public class MD5HelperTests
{
    [Fact]
    public void GetMD5Hash_ReturnsConsistentHash()
    {
        var input = "Hello World";
        var hash1 = input.GetMD5Hash();
        var hash2 = input.GetMD5Hash();

        hash1.Should().Be(hash2);
    }

    [Fact]
    public void GetMD5Hash_Returns32CharacterString()
    {
        var input = "Test";
        var result = input.GetMD5Hash();

        result.Should().HaveLength(32);
    }

    [Fact]
    public void GetMD5Hash_ReturnsHexadecimalString()
    {
        var input = "Test";
        var result = input.GetMD5Hash();

        result.Should().MatchRegex("^[a-f0-9]{32}$");
    }

    [Fact]
    public void GetMD5Hash_KnownValue_ReturnsExpectedHash()
    {
        // MD5 of "test" is 098f6bcd4621d373cade4e832627b4f6
        var result = "test".GetMD5Hash();
        result.Should().Be("098f6bcd4621d373cade4e832627b4f6");
    }

    [Fact]
    public void VerifyMD5Hash_WithCorrectHash_ReturnsTrue()
    {
        var input = "test";
        var hash = "098f6bcd4621d373cade4e832627b4f6";

        var result = MD5Helper.VerifyMD5Hash(input, hash);
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyMD5Hash_WithIncorrectHash_ReturnsFalse()
    {
        var input = "test";
        var wrongHash = "wronghash12345678901234567890ab";

        var result = MD5Helper.VerifyMD5Hash(input, wrongHash);
        result.Should().BeFalse();
    }

    [Fact]
    public void GetMD5Hash_DifferentInputs_ReturnsDifferentHashes()
    {
        var hash1 = "input1".GetMD5Hash();
        var hash2 = "input2".GetMD5Hash();

        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void GetMD5Hash_FromStream_ReturnsHash()
    {
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("test"));
        var result = stream.GetMD5Hash();

        result.Should().HaveLength(32);
    }

    [Fact]
    public void VerifyMD5Hash_Stream_WithCorrectHash_ReturnsTrue()
    {
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("test"));
        var hash = stream.GetMD5Hash();

        using var verifyStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("test"));
        var result = MD5Helper.VerifyMD5Hash(verifyStream, hash);

        result.Should().BeTrue();
    }
}
