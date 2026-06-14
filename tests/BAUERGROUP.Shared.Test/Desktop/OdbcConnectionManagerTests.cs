using BAUERGROUP.Shared.Desktop.DataSources;

namespace BAUERGROUP.Shared.Test.Desktop;

public class OdbcConnectionManagerTests
{
    [Fact]
    public void DataSourceType_Enum_HasSystemValue()
    {
        OdbcConnectionManager.DataSourceType.System.Should().BeDefined();
    }

    [Fact]
    public void DataSourceType_Enum_HasUserValue()
    {
        OdbcConnectionManager.DataSourceType.User.Should().BeDefined();
    }

    [Theory]
    [InlineData(OdbcConnectionManager.DataSourceType.System, 0)]
    [InlineData(OdbcConnectionManager.DataSourceType.User, 1)]
    public void DataSourceType_Enum_HasExpectedIntValues(OdbcConnectionManager.DataSourceType type, int expectedValue)
    {
        ((int)type).Should().Be(expectedValue);
    }

    [Fact]
    public void GetConnectionString_WithValidParameters_ReturnsConnectionString()
    {
        var dsn = "TestDSN";
        var username = "testuser";
        var password = "testpass";

        var result = OdbcConnectionManager.GetConnectionString(dsn, username, password);

        result.Should().NotBeNullOrEmpty();
        result.ToUpper().Should().Contain("DSN=TESTDSN");
        result.ToUpper().Should().Contain("UID=TESTUSER");
        result.ToUpper().Should().Contain("PWD=TESTPASS");
    }

    [Fact]
    public void GetConnectionString_WithEmptyUsername_IncludesEmptyUID()
    {
        var result = OdbcConnectionManager.GetConnectionString("DSN", "", "pass");

        result.ToUpper().Should().Contain("DSN=DSN");
        result.ToUpper().Should().Contain("UID=");
    }

    [Fact]
    public void GetConnectionString_WithEmptyPassword_IncludesEmptyPWD()
    {
        var result = OdbcConnectionManager.GetConnectionString("DSN", "user", "");

        result.ToUpper().Should().Contain("DSN=DSN");
        result.ToUpper().Should().Contain("PWD=");
    }

    [Fact]
    public void GetConnectionString_WithSpecialCharacters_HandlesCorrectly()
    {
        var result = OdbcConnectionManager.GetConnectionString("My DSN", "user@domain", "p@ss=word");

        result.Should().Contain("My DSN");
        result.Should().Contain("user@domain");
    }

    [Fact]
    public void GetDataSourceNames_ReturnsResult()
    {
        // This may return empty if no ODBC sources are configured on the machine
        var result = OdbcConnectionManager.GetDataSourceNames();

        result.Should().NotBeNull();
    }

    [Fact]
    public void GetDataSourceNames_ForUserType_ReturnsResult()
    {
        var result = OdbcConnectionManager.GetDataSourceNames(OdbcConnectionManager.DataSourceType.User);

        result.Should().NotBeNull();
    }

    [Fact]
    public void GetDataSourceNames_ForSystemType_ReturnsResult()
    {
        var result = OdbcConnectionManager.GetDataSourceNames(OdbcConnectionManager.DataSourceType.System);

        result.Should().NotBeNull();
    }
}
