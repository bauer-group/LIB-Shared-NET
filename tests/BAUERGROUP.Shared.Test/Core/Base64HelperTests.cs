using BAUERGROUP.Shared.Core.Extensions;

namespace BAUERGROUP.Shared.Test.Core;

public class Base64HelperTests
{
    [Fact]
    public void Base64Encode_EncodesString()
    {
        var input = "Hello World";
        var result = input.Base64Encode();

        result.Should().NotBeNullOrEmpty();
        result.Should().NotBe(input);
    }

    [Fact]
    public void Base64Decode_DecodesString()
    {
        var encoded = "SGVsbG8gV29ybGQ="; // "Hello World" in Base64 (ASCII)
        var result = encoded.Base64Decode();

        result.Should().Be("Hello World");
    }

    [Fact]
    public void Base64Encode_AndDecode_RoundTrips()
    {
        var original = "Test String 123!@#";
        var encoded = original.Base64Encode();
        var decoded = encoded.Base64Decode();

        decoded.Should().Be(original);
    }

    [Fact]
    public void ByteToBase64_ConvertsBytesToBase64()
    {
        var bytes = new byte[] { 72, 101, 108, 108, 111 }; // "Hello" in ASCII
        var result = Base64Helper.ByteToBase64(bytes);

        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Base64ToByte_ConvertsBase64ToBytes()
    {
        var base64 = "SGVsbG8="; // "Hello" in Base64
        var result = Base64Helper.Base64ToByte(base64);

        result.Should().BeEquivalentTo(new byte[] { 72, 101, 108, 108, 111 });
    }

    [Fact]
    public void ByteToBase64_AndBack_RoundTrips()
    {
        var original = new byte[] { 1, 2, 3, 4, 5, 255, 0, 128 };
        var base64 = Base64Helper.ByteToBase64(original);
        var restored = Base64Helper.Base64ToByte(base64);

        restored.Should().BeEquivalentTo(original);
    }

    [Fact]
    public void Base64ToByte_FromByteArray_ConvertsCorrectly()
    {
        var base64Bytes = System.Text.Encoding.ASCII.GetBytes("SGVsbG8=");
        var result = Base64Helper.Base64ToByte(base64Bytes);

        result.Should().BeEquivalentTo(new byte[] { 72, 101, 108, 108, 111 });
    }
}
