using BAUERGROUP.Shared.Cloud.RemoveBG;
using System.Net;

namespace BAUERGROUP.Shared.Test.Cloud;

public class RemoveBGClientExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_SetsMessage()
    {
        var exception = new RemoveBGClientException("Test error");

        exception.Message.Should().Be("Test error");
        exception.StatusCode.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_SetsBoth()
    {
        var inner = new InvalidOperationException("Inner");
        var exception = new RemoveBGClientException("Outer", inner);

        exception.Message.Should().Be("Outer");
        exception.InnerException.Should().Be(inner);
        exception.StatusCode.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithStatusCodeAndMessage_SetsBoth()
    {
        var exception = new RemoveBGClientException(HttpStatusCode.Unauthorized, "Unauthorized");

        exception.Message.Should().Be("Unauthorized");
        exception.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public void Constructor_WithStatusCodeMessageAndInnerException_SetsAll()
    {
        var inner = new Exception("Inner");
        var exception = new RemoveBGClientException(HttpStatusCode.BadRequest, "Bad request", inner);

        exception.Message.Should().Be("Bad request");
        exception.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        exception.InnerException.Should().Be(inner);
    }

    [Fact]
    public void StatusCode_CanBeNotFound()
    {
        var exception = new RemoveBGClientException(HttpStatusCode.NotFound, "Not found");

        exception.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public void StatusCode_CanBeForbidden()
    {
        var exception = new RemoveBGClientException(HttpStatusCode.Forbidden, "Forbidden");

        exception.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public void StatusCode_CanBeInternalServerError()
    {
        var exception = new RemoveBGClientException(HttpStatusCode.InternalServerError, "Server error");

        exception.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public void CanBeThrown_AndCaught()
    {
        Action action = () => throw new RemoveBGClientException("Test throw");

        action.Should().Throw<RemoveBGClientException>()
            .WithMessage("Test throw");
    }

    [Fact]
    public void DerivesFromException()
    {
        var exception = new RemoveBGClientException("Test");

        exception.Should().BeAssignableTo<Exception>();
    }
}
