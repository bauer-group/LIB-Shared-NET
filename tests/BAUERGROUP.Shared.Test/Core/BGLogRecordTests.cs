using BAUERGROUP.Shared.Core.Logging;
using NLog;

namespace BAUERGROUP.Shared.Test.Core;

public class BGLogRecordTests
{
    private static readonly DateTime SampleTime = new(2026, 9, 16, 13, 5, 7, 42, DateTimeKind.Local);

    [Fact]
    public void DisplayText_ShouldUseInvariantTimestampAndPaddedUppercaseLevel()
    {
        var record = new BGLogRecord(SampleTime, LogLevel.Info, "Some.Logger", "hello world");

        record.DisplayText.Should().Be("13:05:07.042 INFO  hello world");
    }

    [Fact]
    public void DisplayText_WithException_ShouldAppendExceptionOnNewLine()
    {
        var record = new BGLogRecord(SampleTime, LogLevel.Error, "Some.Logger", "boom", "System.InvalidOperationException: boom");

        record.DisplayText.Should().Be(
            "13:05:07.042 ERROR boom" + Environment.NewLine + "System.InvalidOperationException: boom");
    }

    [Fact]
    public void DisplayText_ShouldBeStableAcrossReads()
    {
        var record = new BGLogRecord(SampleTime, LogLevel.Warn, "Some.Logger", "cached");

        var first = record.DisplayText;
        var second = record.DisplayText;

        second.Should().BeSameAs(first);
        record.ToString().Should().Be(first);
    }

    [Fact]
    public void Ctor_NullLevel_ShouldThrow()
    {
        Action act = () => new BGLogRecord(SampleTime, null!, "Some.Logger", "message");

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Ctor_NullTexts_ShouldBecomeEmptyStringsAndNullException()
    {
        var record = new BGLogRecord(SampleTime, LogLevel.Debug, null, null);

        record.LoggerName.Should().BeEmpty();
        record.Message.Should().BeEmpty();
        record.Exception.Should().BeNull();
    }

    [Fact]
    public void Message_LongerThanMaxTextLength_ShouldBeTruncatedAndMarked()
    {
        var message = new string('m', BGLogRecord.MaxTextLength + 500);

        var record = new BGLogRecord(SampleTime, LogLevel.Info, "Some.Logger", message);

        record.Message.Should().HaveLength(BGLogRecord.MaxTextLength + BGLogRecord.TruncationMarker.Length);
        record.Message.Should().EndWith(BGLogRecord.TruncationMarker);
        record.Message.Should().StartWith(new string('m', BGLogRecord.MaxTextLength));
    }

    [Fact]
    public void Exception_LongerThanMaxTextLength_ShouldBeTruncatedAndMarked()
    {
        var exception = new string('e', BGLogRecord.MaxTextLength + 1);

        var record = new BGLogRecord(SampleTime, LogLevel.Error, "Some.Logger", "short", exception);

        record.Exception.Should().HaveLength(BGLogRecord.MaxTextLength + BGLogRecord.TruncationMarker.Length);
        record.Exception.Should().EndWith(BGLogRecord.TruncationMarker);
    }

    [Fact]
    public void Message_AtMaxTextLength_ShouldNotBeTruncated()
    {
        var message = new string('m', BGLogRecord.MaxTextLength);

        var record = new BGLogRecord(SampleTime, LogLevel.Info, "Some.Logger", message);

        record.Message.Should().Be(message);
    }

    [Theory]
    [InlineData("Trace", false, false)]
    [InlineData("Debug", false, false)]
    [InlineData("Info", false, false)]
    [InlineData("Warn", true, false)]
    [InlineData("Error", false, true)]
    [InlineData("Fatal", false, true)]
    public void IsWarning_IsError_ShouldMatchLevel(string levelName, bool isWarning, bool isError)
    {
        var record = new BGLogRecord(SampleTime, LogLevel.FromString(levelName), "Some.Logger", "message");

        record.IsWarning.Should().Be(isWarning);
        record.IsError.Should().Be(isError);
    }
}
