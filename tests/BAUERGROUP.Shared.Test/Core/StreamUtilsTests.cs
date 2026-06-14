using BAUERGROUP.Shared.Core.Streams;

namespace BAUERGROUP.Shared.Test.Core;

public class StreamUtilsTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly List<string> _testFiles = [];

    public StreamUtilsTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"StreamUtilsTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        foreach (var file in _testFiles)
        {
            if (File.Exists(file))
                File.Delete(file);
        }
        if (Directory.Exists(_testDirectory))
            Directory.Delete(_testDirectory, true);
    }

    [Fact]
    public void CopyToMemoryStream_CopiesDataCorrectly()
    {
        var originalData = new byte[] { 1, 2, 3, 4, 5 };
        using var inputStream = new MemoryStream(originalData);

        using var result = inputStream.CopyToMemoryStream();

        result.Should().NotBeNull();
        result.ToArray().Should().BeEquivalentTo(originalData);
    }

    [Fact]
    public void CopyToMemoryStream_SetsPositionToBeginning()
    {
        var originalData = new byte[] { 1, 2, 3 };
        using var inputStream = new MemoryStream(originalData);

        using var result = inputStream.CopyToMemoryStream();

        result.Position.Should().Be(0);
    }

    [Fact]
    public void CopyToMemoryStream_WithEmptyStream_ReturnsEmptyMemoryStream()
    {
        using var inputStream = new MemoryStream();

        using var result = inputStream.CopyToMemoryStream();

        result.Length.Should().Be(0);
    }

    [Fact]
    public void CopyToMemoryStream_WithLargeData_CopiesCorrectly()
    {
        var largeData = new byte[100000];
        new Random(42).NextBytes(largeData);
        using var inputStream = new MemoryStream(largeData);

        using var result = inputStream.CopyToMemoryStream();

        result.ToArray().Should().BeEquivalentTo(largeData);
    }

    [Fact]
    public void CopyToBytes_ReturnsCorrectBytes()
    {
        var originalData = new byte[] { 10, 20, 30, 40 };
        using var inputStream = new MemoryStream(originalData);

        var result = inputStream.CopyToBytes();

        result.Should().BeEquivalentTo(originalData);
    }

    [Fact]
    public void CopyToBytes_WithEmptyStream_ReturnsEmptyArray()
    {
        using var inputStream = new MemoryStream();

        var result = inputStream.CopyToBytes();

        result.Should().BeEmpty();
    }

    [Fact]
    public void CopyToBytes_FromFileStream_ReadsCorrectly()
    {
        var filePath = Path.Combine(_testDirectory, "copy_bytes_test.bin");
        _testFiles.Add(filePath);
        var fileData = new byte[] { 0xAA, 0xBB, 0xCC };
        File.WriteAllBytes(filePath, fileData);

        using var fileStream = File.OpenRead(filePath);
        var result = fileStream.CopyToBytes();

        result.Should().BeEquivalentTo(fileData);
    }

    [Fact]
    public void CopyToMemoryStream_FromPartiallyReadStream_CopiesRemainingData()
    {
        var originalData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        using var inputStream = new MemoryStream(originalData);
        inputStream.ReadByte(); // Read first byte
        inputStream.ReadByte(); // Read second byte

        using var result = inputStream.CopyToMemoryStream();

        result.ToArray().Should().BeEquivalentTo(new byte[] { 3, 4, 5, 6, 7, 8 });
    }
}
