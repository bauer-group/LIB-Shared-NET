using BAUERGROUP.Shared.Core.Extensions;

namespace BAUERGROUP.Shared.Test.Core;

public class BinaryHelperTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly List<string> _testFiles = [];

    public BinaryHelperTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"BinaryHelperTests_{Guid.NewGuid()}");
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

    private string CreateTestFile(string fileName)
    {
        var path = Path.Combine(_testDirectory, fileName);
        _testFiles.Add(path);
        return path;
    }

    [Fact]
    public void WriteBytesToFile_WritesDataCorrectly()
    {
        var filePath = CreateTestFile("write_test.bin");
        var data = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };

        BinaryHelper.WriteBytesToFile(filePath, data);

        File.Exists(filePath).Should().BeTrue();
        var writtenData = File.ReadAllBytes(filePath);
        writtenData.Should().BeEquivalentTo(data);
    }

    [Fact]
    public void WriteBytesToFile_WithEmptyArray_CreatesEmptyFile()
    {
        var filePath = CreateTestFile("empty_test.bin");
        var data = Array.Empty<byte>();

        BinaryHelper.WriteBytesToFile(filePath, data);

        File.Exists(filePath).Should().BeTrue();
        new FileInfo(filePath).Length.Should().Be(0);
    }

    [Fact]
    public void ReadBytesFromFile_ReadsDataCorrectly()
    {
        var filePath = CreateTestFile("read_test.bin");
        var expectedData = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD };
        File.WriteAllBytes(filePath, expectedData);

        var result = BinaryHelper.ReadBytesFromFile(filePath);

        result.Should().BeEquivalentTo(expectedData);
    }

    [Fact]
    public void ReadBytesFromFile_WithLargeFile_ReadsCorrectly()
    {
        var filePath = CreateTestFile("large_test.bin");
        var largeData = new byte[1024 * 1024]; // 1MB
        new Random(42).NextBytes(largeData);
        File.WriteAllBytes(filePath, largeData);

        var result = BinaryHelper.ReadBytesFromFile(filePath);

        result.Should().BeEquivalentTo(largeData);
    }

    [Fact]
    public void WriteBytesToFile_OverwritesExistingFile()
    {
        var filePath = CreateTestFile("overwrite_test.bin");
        var initialData = new byte[] { 0x01, 0x02 };
        var newData = new byte[] { 0x03, 0x04, 0x05 };
        File.WriteAllBytes(filePath, initialData);

        BinaryHelper.WriteBytesToFile(filePath, newData);

        var result = File.ReadAllBytes(filePath);
        result.Should().BeEquivalentTo(newData);
    }

    [Fact]
    public void ReadBytesFromFile_NonExistentFile_ThrowsException()
    {
        var filePath = Path.Combine(_testDirectory, "nonexistent.bin");

        var action = () => BinaryHelper.ReadBytesFromFile(filePath);

        action.Should().Throw<FileNotFoundException>();
    }

    [Fact]
    public void RoundTrip_PreservesData()
    {
        var filePath = CreateTestFile("roundtrip_test.bin");
        var originalData = new byte[] { 0x00, 0x7F, 0x80, 0xFF };

        BinaryHelper.WriteBytesToFile(filePath, originalData);
        var result = BinaryHelper.ReadBytesFromFile(filePath);

        result.Should().BeEquivalentTo(originalData);
    }
}
