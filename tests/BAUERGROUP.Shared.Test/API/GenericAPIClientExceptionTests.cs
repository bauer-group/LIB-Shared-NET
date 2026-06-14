using BAUERGROUP.Shared.API.API;

namespace BAUERGROUP.Shared.Test.API;

public class GenericAPIClientExceptionTests
{
    [Fact]
    public void Constructor_Default_CreatesException()
    {
        var exception = new GenericAPIClientException();

        exception.Should().NotBeNull();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Constructor_WithMessage_SetsMessage()
    {
        var message = "Test error message";

        var exception = new GenericAPIClientException(message);

        exception.Message.Should().Be(message);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_SetsBoth()
    {
        var message = "Outer error";
        var innerException = new InvalidOperationException("Inner error");

        var exception = new GenericAPIClientException(message, innerException);

        exception.Message.Should().Be(message);
        exception.InnerException.Should().Be(innerException);
    }

    [Fact]
    public void InnerException_CanBeAccessed()
    {
        var inner = new ArgumentException("Inner");
        var exception = new GenericAPIClientException("Outer", inner);

        exception.InnerException.Should().BeOfType<ArgumentException>();
        exception.InnerException!.Message.Should().Be("Inner");
    }

    [Fact]
    public void IsException_DerivesFromException()
    {
        var exception = new GenericAPIClientException();

        exception.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void CanBeThrown_AndCaught()
    {
        Action action = () => throw new GenericAPIClientException("Test throw");

        action.Should().Throw<GenericAPIClientException>()
            .WithMessage("Test throw");
    }

    [Fact]
    public void CanBeCaughtAsException()
    {
        Action action = () => throw new GenericAPIClientException("Generic catch");

        action.Should().Throw<Exception>();
    }
}
