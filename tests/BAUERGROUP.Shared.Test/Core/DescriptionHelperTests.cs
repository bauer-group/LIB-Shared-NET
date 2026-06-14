using BAUERGROUP.Shared.Core.Extensions;
using System.ComponentModel;

namespace BAUERGROUP.Shared.Test.Core;

public class DescriptionHelperTests
{
    public enum TestEnumWithDescription
    {
        [Description("First Item Description")]
        FirstItem,

        [Description("Second Item Description")]
        SecondItem,

        [Description("This is a longer description for testing")]
        LongDescription
    }

    public enum TestEnumWithoutDescription
    {
        NoDescription
    }

    [Fact]
    public void GetDescriptionAttribute_WithDescription_ReturnsDescription()
    {
        var value = TestEnumWithDescription.FirstItem;

        var result = value.GetDescriptionAttribute();

        result.Should().Be("First Item Description");
    }

    [Fact]
    public void GetDescriptionAttribute_WithDifferentValue_ReturnsCorrectDescription()
    {
        var value = TestEnumWithDescription.SecondItem;

        var result = value.GetDescriptionAttribute();

        result.Should().Be("Second Item Description");
    }

    [Fact]
    public void GetDescriptionAttribute_WithLongDescription_ReturnsFullDescription()
    {
        var value = TestEnumWithDescription.LongDescription;

        var result = value.GetDescriptionAttribute();

        result.Should().Be("This is a longer description for testing");
    }

    [Fact]
    public void GetAttributeOfType_WithDescriptionAttribute_ReturnsAttribute()
    {
        var value = TestEnumWithDescription.FirstItem;

        var result = value.GetAttributeOfType<DescriptionAttribute>();

        result.Should().NotBeNull();
        result.Description.Should().Be("First Item Description");
    }

    [Fact]
    public void GetDescriptionAttribute_WithoutDescription_ThrowsException()
    {
        var value = TestEnumWithoutDescription.NoDescription;

        var action = () => value.GetDescriptionAttribute();

        action.Should().Throw<IndexOutOfRangeException>();
    }

    [Fact]
    public void GetAttributeOfType_WithCustomAttribute_ReturnsAttribute()
    {
        var value = TestEnumWithCustomAttribute.WithCustom;

        var result = value.GetAttributeOfType<CustomTestAttribute>();

        result.Should().NotBeNull();
        result.CustomValue.Should().Be("CustomData");
    }

    public enum TestEnumWithCustomAttribute
    {
        [CustomTest("CustomData")]
        WithCustom
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class CustomTestAttribute : Attribute
    {
        public string CustomValue { get; }

        public CustomTestAttribute(string value)
        {
            CustomValue = value;
        }
    }
}
