using BAUERGROUP.Shared.Desktop.Application;

namespace BAUERGROUP.Shared.Test.Desktop;

public class DesktopEnvironmentPropertiesTests
{
    [Fact]
    public void ProcessIdentity_ReturnsCurrentWindowsIdentity()
    {
        var identity = DesktopEnvironmentProperties.ProcessIdentity;

        identity.Should().NotBeNull();
        identity.Name.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ProcessIdentity_HasAuthenticationType()
    {
        var identity = DesktopEnvironmentProperties.ProcessIdentity;

        identity.AuthenticationType.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ProcessIdentity_IsAuthenticated()
    {
        var identity = DesktopEnvironmentProperties.ProcessIdentity;

        identity.IsAuthenticated.Should().BeTrue();
    }

    [Fact]
    public void ProcessIdentity_ReturnsSameIdentityOnMultipleCalls()
    {
        var identity1 = DesktopEnvironmentProperties.ProcessIdentity;
        var identity2 = DesktopEnvironmentProperties.ProcessIdentity;

        identity1.Name.Should().Be(identity2.Name);
    }
}
