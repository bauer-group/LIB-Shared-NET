using BAUERGROUP.Shared.Core.Extensions;

namespace BAUERGROUP.Shared.Test.Core;

public class FilenameHelperTests
{
    [Fact]
    public void ToValidFilename_ValidFilename_ReturnsUnchanged()
    {
        var input = "valid_filename.txt";
        var result = input.ToValidFilename();

        result.Should().Be("valid_filename.txt");
    }

    [Fact]
    public void ToValidFilename_WithInvalidChars_ReplacesWithUnderscore()
    {
        var input = "file<name>.txt";
        var result = input.ToValidFilename();

        result.Should().NotContain("<");
        result.Should().NotContain(">");
        result.Should().Contain("_");
    }

    [Fact]
    public void ToValidFilename_WithCustomReplacement_UsesCustomChar()
    {
        var input = "file:name.txt";
        var result = input.ToValidFilename('-');

        result.Should().Contain("-");
        result.Should().NotContain(":");
    }

    [Fact]
    public void ToValidFilename_WithSlashes_ReplacesSlashes()
    {
        var input = "path/to/file.txt";
        var result = input.ToValidFilename();

        result.Should().NotContain("/");
    }

    [Fact]
    public void ToValidFilename_WithMultipleInvalidChars_ReplacesAll()
    {
        var input = "file<>:\"/\\|?*.txt";
        var result = input.ToValidFilename();

        foreach (var invalidChar in Path.GetInvalidFileNameChars())
        {
            result.Should().NotContain(invalidChar.ToString());
        }
    }

    [Fact]
    public void ToValidFilename_EmptyString_ReturnsEmpty()
    {
        var input = "";
        var result = input.ToValidFilename();

        result.Should().BeEmpty();
    }
}
