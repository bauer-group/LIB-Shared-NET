using BAUERGROUP.Shared.Core.Extensions;

namespace BAUERGROUP.Shared.Test.Core;

public class ExceptionHelperTests
{
    [Fact]
    public void GetExceptionDetails_WithSimpleException_ReturnsDetails()
    {
        var exception = new InvalidOperationException("Test error message");

        var result = exception.GetExceptionDetails();

        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("Message");
        result.Should().Contain("Test error message");
    }

    [Fact]
    public void GetExceptionDetails_WithInnerException_IncludesInnerException()
    {
        var innerException = new ArgumentException("Inner error");
        var exception = new InvalidOperationException("Outer error", innerException);

        var result = exception.GetExceptionDetails();

        result.Should().Contain("InnerException");
    }

    [Fact]
    public void GetExceptionDetails_ContainsExceptionType()
    {
        var exception = new ArgumentNullException("paramName");

        var result = exception.GetExceptionDetails();

        result.Should().Contain("ArgumentNullException");
    }

    [Fact]
    public void GetExceptionDetails_WithStackTrace_IncludesStackTrace()
    {
        Exception? exception = null;
        try
        {
            throw new InvalidOperationException("With stack trace");
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        var result = exception!.GetExceptionDetails();

        result.Should().Contain("StackTrace");
    }

    [Fact]
    public void GetExceptionDetails_ContainsSource()
    {
        var exception = new Exception("Test");

        var result = exception.GetExceptionDetails();

        result.Should().Contain("Source");
    }

    [Fact]
    public void GetExceptionDetails_WithCustomException_IncludesCustomProperties()
    {
        var exception = new CustomTestException("Custom error", 42);

        var result = exception.GetExceptionDetails();

        result.Should().Contain("ErrorCode");
        result.Should().Contain("42");
    }

    [Fact]
    public void GetExceptionDetails_FormatsMultipleProperties()
    {
        var exception = new Exception("Test message");

        var result = exception.GetExceptionDetails();

        // Should have multiple lines (one per property)
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        lines.Length.Should().BeGreaterThan(1);
    }

    [Fact]
    public void GetExceptionDetails_WithNullPropertyValue_HandlesGracefully()
    {
        var exception = new Exception(); // No message set

        var action = () => exception.GetExceptionDetails();

        action.Should().NotThrow();
    }

    private class CustomTestException : Exception
    {
        public int ErrorCode { get; }

        public CustomTestException(string message, int errorCode) : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}
