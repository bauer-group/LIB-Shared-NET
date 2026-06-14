using BAUERGROUP.Shared.Cloud.FixerIO;
using System.Text.Json;

namespace BAUERGROUP.Shared.Test.Cloud;

public class FixerIODataTests
{
    [Fact]
    public void FixerIODataBase_Success_CanBeSet()
    {
        var data = new FixerIODataBase { Success = true };

        data.Success.Should().BeTrue();
    }

    [Fact]
    public void FixerIODataBase_Error_CanBeSet()
    {
        var error = new FixerIODataBase.ErrorInfo
        {
            Code = 101,
            Type = "invalid_access_key",
            Info = "No API Key was specified."
        };
        var data = new FixerIODataBase { Error = error };

        data.Error.Should().NotBeNull();
        data.Error.Code.Should().Be(101);
        data.Error.Type.Should().Be("invalid_access_key");
        data.Error.Info.Should().Be("No API Key was specified.");
    }

    [Fact]
    public void FixerIODataBase_CanBeDeserialized()
    {
        var json = """
        {
            "success": false,
            "error": {
                "code": 101,
                "type": "invalid_access_key",
                "info": "Invalid API key"
            }
        }
        """;

        var data = JsonSerializer.Deserialize<FixerIODataBase>(json);

        data.Should().NotBeNull();
        data!.Success.Should().BeFalse();
        data.Error.Should().NotBeNull();
        data.Error!.Code.Should().Be(101);
    }

    [Fact]
    public void FixerIODataRates_Properties_CanBeSet()
    {
        var rates = new FixerIODataRates
        {
            Success = true,
            Timestamp = 1704067200,
            Base = "EUR",
            Date = new DateTime(2024, 1, 1),
            Rates = new Dictionary<string, decimal>
            {
                { "USD", 1.1m },
                { "GBP", 0.85m },
                { "CHF", 0.95m }
            }
        };

        rates.Success.Should().BeTrue();
        rates.Timestamp.Should().Be(1704067200);
        rates.Base.Should().Be("EUR");
        rates.Date.Should().Be(new DateTime(2024, 1, 1));
        rates.Rates.Should().HaveCount(3);
        rates.Rates["USD"].Should().Be(1.1m);
    }

    [Fact]
    public void FixerIODataRates_CanBeDeserialized()
    {
        var json = """
        {
            "success": true,
            "timestamp": 1704067200,
            "base": "EUR",
            "date": "2024-01-01",
            "rates": {
                "USD": 1.10,
                "GBP": 0.85,
                "CHF": 0.95
            }
        }
        """;

        var data = JsonSerializer.Deserialize<FixerIODataRates>(json);

        data.Should().NotBeNull();
        data!.Success.Should().BeTrue();
        data.Base.Should().Be("EUR");
        data.Rates.Should().ContainKey("USD");
    }

    [Fact]
    public void FixerIODataSymbols_Properties_CanBeSet()
    {
        var symbols = new FixerIODataSymbols
        {
            Success = true,
            Symbols = new Dictionary<string, string>
            {
                { "EUR", "Euro" },
                { "USD", "United States Dollar" },
                { "GBP", "British Pound Sterling" }
            }
        };

        symbols.Success.Should().BeTrue();
        symbols.Symbols.Should().HaveCount(3);
        symbols.Symbols["EUR"].Should().Be("Euro");
    }

    [Fact]
    public void FixerIODataSymbols_CanBeDeserialized()
    {
        var json = """
        {
            "success": true,
            "symbols": {
                "EUR": "Euro",
                "USD": "United States Dollar"
            }
        }
        """;

        var data = JsonSerializer.Deserialize<FixerIODataSymbols>(json);

        data.Should().NotBeNull();
        data!.Success.Should().BeTrue();
        data.Symbols.Should().ContainKey("EUR");
        data.Symbols["EUR"].Should().Be("Euro");
    }

    [Fact]
    public void FixerIODataRates_InheritsFromBase()
    {
        var rates = new FixerIODataRates();

        rates.Should().BeAssignableTo<FixerIODataBase>();
    }

    [Fact]
    public void FixerIODataSymbols_InheritsFromBase()
    {
        var symbols = new FixerIODataSymbols();

        symbols.Should().BeAssignableTo<FixerIODataBase>();
    }
}
