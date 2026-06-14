using BAUERGROUP.Shared.Cloud.FixerIO;

namespace BAUERGROUP.Shared.Test.Cloud;

public class FixerIOClientExceptionTests
{
    [Fact]
    public void DefaultConstructor_CreatesException()
    {
        var exception = new FixerIOClientException();

        exception.Should().NotBeNull();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Constructor_WithMessage_SetsMessage()
    {
        var exception = new FixerIOClientException("Test error");

        exception.Message.Should().Be("Test error");
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_SetsBoth()
    {
        var inner = new InvalidOperationException("Inner");
        var exception = new FixerIOClientException("Outer", inner);

        exception.Message.Should().Be("Outer");
        exception.InnerException.Should().Be(inner);
    }

    [Fact]
    public void DerivesFromException()
    {
        var exception = new FixerIOClientException();

        exception.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void CanBeThrown_AndCaught()
    {
        Action action = () => throw new FixerIOClientException("Test throw");

        action.Should().Throw<FixerIOClientException>()
            .WithMessage("Test throw");
    }
}
