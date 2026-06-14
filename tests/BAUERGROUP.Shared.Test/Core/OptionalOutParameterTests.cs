using BAUERGROUP.Shared.Core.Utilities;

namespace BAUERGROUP.Shared.Test.Core;

public class OptionalOutParameterTests
{
    [Fact]
    public void Result_CanBeSet()
    {
        var parameter = new OptionalOutParameter<string>();

        parameter.Result = "Test Value";

        parameter.Result.Should().Be("Test Value");
    }

    [Fact]
    public void Result_DefaultIsNull()
    {
        var parameter = new OptionalOutParameter<string>();

        parameter.Result.Should().BeNull();
    }

    [Fact]
    public void Result_DefaultForValueType_IsDefaultValue()
    {
        var parameter = new OptionalOutParameter<int>();

        parameter.Result.Should().Be(0);
    }

    [Fact]
    public void Result_CanStoreComplexTypes()
    {
        var parameter = new OptionalOutParameter<List<int>>();
        var list = new List<int> { 1, 2, 3 };

        parameter.Result = list;

        parameter.Result.Should().BeSameAs(list);
    }

    [Fact]
    public void Result_CanBeOverwritten()
    {
        var parameter = new OptionalOutParameter<int>();
        parameter.Result = 10;
        parameter.Result = 20;

        parameter.Result.Should().Be(20);
    }

    [Fact]
    public void CanBeUsedAsOptionalParameter()
    {
        var output = new OptionalOutParameter<string>();
        MethodWithOptionalOut(output);

        output.Result.Should().Be("Success");
    }

    [Fact]
    public void CanBeNull_ForOptionalBehavior()
    {
        MethodWithOptionalOut(null);
        // No exception should be thrown
    }

    private void MethodWithOptionalOut(OptionalOutParameter<string>? output)
    {
        if (output != null)
        {
            output.Result = "Success";
        }
    }

    [Fact]
    public void WorksWithNullableTypes()
    {
        var parameter = new OptionalOutParameter<int?>();

        parameter.Result = null;
        parameter.Result.Should().BeNull();

        parameter.Result = 42;
        parameter.Result.Should().Be(42);
    }
}
