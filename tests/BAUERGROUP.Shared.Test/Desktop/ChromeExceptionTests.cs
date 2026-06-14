using BAUERGROUP.Shared.Desktop.Exceptions;

namespace BAUERGROUP.Shared.Test.Desktop;

public class ChromeExceptionTests
{
    [Fact]
    public void DefaultConstructor_CreatesException()
    {
        var exception = new ChromeException();

        exception.Should().NotBeNull();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Constructor_WithMessage_SetsMessage()
    {
        var exception = new ChromeException("Chrome error occurred");

        exception.Message.Should().Be("Chrome error occurred");
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_SetsBoth()
    {
        var inner = new InvalidOperationException("Inner error");
        var exception = new ChromeException("Outer error", inner);

        exception.Message.Should().Be("Outer error");
        exception.InnerException.Should().Be(inner);
    }

    [Fact]
    public void DerivesFromException()
    {
        var exception = new ChromeException();

        exception.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void CanBeThrown_AndCaught()
    {
        Action action = () => throw new ChromeException("Test throw");

        action.Should().Throw<ChromeException>()
            .WithMessage("Test throw");
    }

    [Fact]
    public void InnerException_CanBeAccessed()
    {
        var inner = new ArgumentException("Inner arg");
        var exception = new ChromeException("Outer", inner);

        exception.InnerException.Should().BeOfType<ArgumentException>();
        exception.InnerException!.Message.Should().Be("Inner arg");
    }
}
