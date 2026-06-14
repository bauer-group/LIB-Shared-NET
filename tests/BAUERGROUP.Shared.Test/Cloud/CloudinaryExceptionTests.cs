using BAUERGROUP.Shared.Cloud.CloudinaryClient;
using System.Net;

namespace BAUERGROUP.Shared.Test.Cloud;

public class CloudinaryExceptionTests
{
    [Fact]
    public void DefaultConstructor_SetsInternalServerError()
    {
        var exception = new CloudinaryException();

        exception.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public void Constructor_WithMessage_SetsMessageAndDefaultStatusCode()
    {
        var exception = new CloudinaryException("Test error");

        exception.Message.Should().Be("Test error");
        exception.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_SetsBoth()
    {
        var inner = new InvalidOperationException("Inner");
        var exception = new CloudinaryException("Outer", inner);

        exception.Message.Should().Be("Outer");
        exception.InnerException.Should().Be(inner);
    }

    [Fact]
    public void Constructor_WithStatusCodeAndMessage_SetsBoth()
    {
        var exception = new CloudinaryException(HttpStatusCode.NotFound, "Not found");

        exception.Message.Should().Be("Not found");
        exception.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public void Constructor_WithStatusCodeOnly_SetsStatusCode()
    {
        var exception = new CloudinaryException(HttpStatusCode.Unauthorized);

        exception.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public void Constructor_WithStatusCodeMessageAndInnerException_SetsAll()
    {
        var inner = new Exception("Inner");
        var exception = new CloudinaryException(HttpStatusCode.BadRequest, "Bad request", inner);

        exception.Message.Should().Be("Bad request");
        exception.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        exception.InnerException.Should().Be(inner);
    }

    [Fact]
    public void StatusCode_CanBeModified()
    {
        var exception = new CloudinaryException("Test");

        exception.StatusCode = HttpStatusCode.Forbidden;

        exception.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public void DerivesFromException()
    {
        var exception = new CloudinaryException();

        exception.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void CanBeThrown_AndCaught()
    {
        Action action = () => throw new CloudinaryException("Test throw");

        action.Should().Throw<CloudinaryException>()
            .WithMessage("Test throw");
    }
}
