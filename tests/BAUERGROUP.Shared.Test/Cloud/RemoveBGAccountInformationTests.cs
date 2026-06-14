using BAUERGROUP.Shared.Cloud.RemoveBG;
using System.Text.Json;

namespace BAUERGROUP.Shared.Test.Cloud;

public class RemoveBGAccountInformationTests
{
    [Fact]
    public void DefaultConstructor_CreatesEmptyRecord()
    {
        var info = new RemoveBGAccountInformation();

        info.Credits.Should().BeNull();
        info.Api.Should().BeNull();
    }

    [Fact]
    public void Credits_CanBeSet()
    {
        var credits = new RemoveBGAccountInformation.CreditInfo
        {
            Total = 100,
            Subscription = 50,
            PayAsYouGo = 30,
            Enterprise = 20
        };

        var info = new RemoveBGAccountInformation { Credits = credits };

        info.Credits.Should().NotBeNull();
        info.Credits!.Total.Should().Be(100);
        info.Credits.Subscription.Should().Be(50);
        info.Credits.PayAsYouGo.Should().Be(30);
        info.Credits.Enterprise.Should().Be(20);
    }

    [Fact]
    public void Api_CanBeSet()
    {
        var api = new RemoveBGAccountInformation.ApiInfo
        {
            FreeCalls = 50,
            Sizes = "preview,full"
        };

        var info = new RemoveBGAccountInformation { Api = api };

        info.Api.Should().NotBeNull();
        info.Api!.FreeCalls.Should().Be(50);
        info.Api.Sizes.Should().Be("preview,full");
    }

    [Fact]
    public void CanBeDeserialized_FromJson()
    {
        var json = """
        {
            "credits": {
                "total": 200,
                "subscription": 100,
                "payg": 75,
                "enterprise": 25
            },
            "api": {
                "free_calls": 10,
                "sizes": "preview,full"
            }
        }
        """;

        var info = JsonSerializer.Deserialize<RemoveBGAccountInformation>(json);

        info.Should().NotBeNull();
        info!.Credits.Should().NotBeNull();
        info.Credits!.Total.Should().Be(200);
        info.Credits.Subscription.Should().Be(100);
        info.Credits.PayAsYouGo.Should().Be(75);
        info.Credits.Enterprise.Should().Be(25);
        info.Api.Should().NotBeNull();
        info.Api!.FreeCalls.Should().Be(10);
        info.Api.Sizes.Should().Be("preview,full");
    }

    [Fact]
    public void CanBeSerialized_ToJson()
    {
        var info = new RemoveBGAccountInformation
        {
            Credits = new RemoveBGAccountInformation.CreditInfo
            {
                Total = 100,
                Subscription = 50
            },
            Api = new RemoveBGAccountInformation.ApiInfo
            {
                FreeCalls = 25,
                Sizes = "full"
            }
        };

        var json = JsonSerializer.Serialize(info);

        json.Should().Contain("credits");
        json.Should().Contain("api");
    }

    [Fact]
    public void CreditInfo_DefaultValues()
    {
        var credits = new RemoveBGAccountInformation.CreditInfo();

        credits.Total.Should().Be(0);
        credits.Subscription.Should().Be(0);
        credits.PayAsYouGo.Should().Be(0);
        credits.Enterprise.Should().Be(0);
    }

    [Fact]
    public void ApiInfo_DefaultValues()
    {
        var api = new RemoveBGAccountInformation.ApiInfo();

        api.FreeCalls.Should().Be(0);
        api.Sizes.Should().BeNull();
    }
}
