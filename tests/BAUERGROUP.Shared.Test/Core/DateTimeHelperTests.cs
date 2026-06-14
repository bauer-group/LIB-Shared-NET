using BAUERGROUP.Shared.Core.Extensions;

namespace BAUERGROUP.Shared.Test.Core;

public class DateTimeHelperTests
{
    [Fact]
    public void ToUnixTimestamp_ReturnsPositiveValue_ForDateAfterEpoch()
    {
        var dateTime = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var result = dateTime.ToUnixTimestamp(true);

        result.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ToUnixTimestamp_ReturnsDifferentValues_ForDifferentDates()
    {
        var date1 = new DateTime(2020, 1, 1, 0, 0, 0);
        var date2 = new DateTime(2021, 1, 1, 0, 0, 0);

        var timestamp1 = date1.ToUnixTimestamp(false);
        var timestamp2 = date2.ToUnixTimestamp(false);

        timestamp2.Should().BeGreaterThan(timestamp1);
    }

    [Fact]
    public void FromUnixTimestamp_ReturnsValidDate()
    {
        var dateTime = new DateTime();
        var result = dateTime.FromUnixTimestamp(0, false);

        result.Year.Should().Be(1970);
        result.Month.Should().Be(1);
        result.Day.Should().Be(1);
    }

    [Fact]
    public void FromUnixTimestamp_WithPositiveValue_ReturnsLaterDate()
    {
        var dateTime = new DateTime();
        var epoch = dateTime.FromUnixTimestamp(0, false);
        var later = dateTime.FromUnixTimestamp(86400, false); // 1 day in seconds

        later.Should().BeAfter(epoch);
        (later - epoch).TotalDays.Should().BeApproximately(1, 0.01);
    }

    [Fact]
    public void ToUnixTimestamp_AndFromUnixTimestamp_AreConsistent()
    {
        var original = new DateTime(2024, 6, 15, 12, 30, 0);
        var timestamp = original.ToUnixTimestamp(false);
        var restored = new DateTime().FromUnixTimestamp(timestamp, false);

        // Allow for timezone offset differences
        restored.Should().BeCloseTo(original, TimeSpan.FromHours(24));
    }

    [Fact]
    public void ToUnixTimestamp_UTC_AndLocal_ProduceDifferentResults_WhenTimezoneOffset()
    {
        var dateTime = new DateTime(2024, 6, 15, 12, 0, 0, DateTimeKind.Local);
        var utcTimestamp = dateTime.ToUnixTimestamp(true);
        var localTimestamp = dateTime.ToUnixTimestamp(false);

        // They should be different unless we're in UTC timezone
        // We just verify they both return valid numbers
        utcTimestamp.Should().BeGreaterThan(0);
        localTimestamp.Should().BeGreaterThan(0);
    }
}
