using Stu.Transformation;
using Xunit;

namespace Stu.Tests.Transformation;

public class UnitConverterTests
{
    private readonly UnitConverter _converter = new(baseFontSize: 16);

    [Theory]
    [InlineData("1rem", "16px")]
    [InlineData("0.5rem", "8px")]
    [InlineData("0.25rem", "4px")]
    [InlineData("0.125rem", "2px")]
    [InlineData("2.5rem", "40px")]
    [InlineData("0rem", "0")]
    public void ConvertRem(string input, string expected)
    {
        Assert.Equal(expected, _converter.Convert(input));
    }

    [Theory]
    [InlineData("1em", "16px")]
    [InlineData("0.875em", "14px")]
    public void ConvertEm(string input, string expected)
    {
        Assert.Equal(expected, _converter.Convert(input));
    }

    [Theory]
    [InlineData("0.5rem 1rem", "8px 16px")]
    [InlineData("0 0.25rem 0.5rem 1rem", "0 4px 8px 16px")]
    public void ConvertCompoundValues(string input, string expected)
    {
        Assert.Equal(expected, _converter.Convert(input));
    }

    [Theory]
    [InlineData("10px", "10px")]
    [InlineData("50%", "50%")]
    [InlineData("100vw", "100vw")]
    [InlineData("auto", "auto")]
    public void PreserveNonRemValues(string input, string expected)
    {
        Assert.Equal(expected, _converter.Convert(input));
    }

    [Fact]
    public void NegativeRem()
    {
        Assert.Equal("-8px", _converter.Convert("-0.5rem"));
    }

    [Fact]
    public void CustomBaseFontSize()
    {
        var converter = new UnitConverter(baseFontSize: 14);
        Assert.Equal("14px", converter.Convert("1rem"));
        Assert.Equal("7px", converter.Convert("0.5rem"));
    }
}
