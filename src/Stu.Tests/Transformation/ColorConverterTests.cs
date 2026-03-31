using Stu.Transformation;
using Xunit;

namespace Stu.Tests.Transformation;

public class ColorConverterTests
{
    private readonly ColorConverter _converter = new();

    [Fact]
    public void ConvertRgb()
    {
        Assert.Equal("#FF0000", _converter.Convert("rgb(255 0 0)"));
        Assert.Equal("#00FF00", _converter.Convert("rgb(0 255 0)"));
        Assert.Equal("#0000FF", _converter.Convert("rgb(0 0 255)"));
        Assert.Equal("#000000", _converter.Convert("rgb(0 0 0)"));
        Assert.Equal("#FFFFFF", _converter.Convert("rgb(255 255 255)"));
    }

    [Fact]
    public void ConvertRgbWithAlpha()
    {
        Assert.Equal("#FF000080", _converter.Convert("rgb(255 0 0 / 0.5)"));
    }

    [Fact]
    public void ConvertRgbCommaSyntax()
    {
        Assert.Equal("#FF8000", _converter.Convert("rgb(255, 128, 0)"));
    }

    [Fact]
    public void ConvertHsl()
    {
        // hsl(0, 100%, 50%) = red
        var result = _converter.Convert("hsl(0 100% 50%)");
        Assert.Equal("#FF0000", result);
    }

    [Fact]
    public void ConvertHslGreen()
    {
        // hsl(120, 100%, 50%) = green
        var result = _converter.Convert("hsl(120 100% 50%)");
        Assert.Equal("#00FF00", result);
    }

    [Fact]
    public void ConvertOklchBlack()
    {
        // oklch(0 0 0) = black
        var result = _converter.Convert("oklch(0 0 0)");
        Assert.Equal("#000000", result);
    }

    [Fact]
    public void ConvertOklchWhite()
    {
        // oklch(1 0 0) = white
        var result = _converter.Convert("oklch(1 0 0)");
        Assert.Equal("#FFFFFF", result);
    }

    [Fact]
    public void ConvertOklchWithAlpha()
    {
        var result = _converter.Convert("oklch(1 0 0 / 0.5)");
        Assert.Equal("#FFFFFF80", result);
    }

    [Fact]
    public void OklchToSrgbBlack()
    {
        var (r, g, b) = ColorConverter.OklchToSrgb(0, 0, 0);
        Assert.Equal(0.0, r, 3);
        Assert.Equal(0.0, g, 3);
        Assert.Equal(0.0, b, 3);
    }

    [Fact]
    public void OklchToSrgbWhite()
    {
        var (r, g, b) = ColorConverter.OklchToSrgb(1, 0, 0);
        Assert.Equal(1.0, r, 2);
        Assert.Equal(1.0, g, 2);
        Assert.Equal(1.0, b, 2);
    }

    [Fact]
    public void ConvertOklchNoneKeyword()
    {
        // oklch(0 0 none) — "none" treated as 0
        var result = _converter.Convert("oklch(0 0 none)");
        Assert.Equal("#000000", result);
    }

    [Fact]
    public void PreserveHexValues()
    {
        Assert.Equal("#FF0000", _converter.Convert("#FF0000"));
    }

    [Fact]
    public void PreserveNamedColors()
    {
        Assert.Equal("transparent", _converter.Convert("transparent"));
        Assert.Equal("currentColor", _converter.Convert("currentColor"));
        Assert.Equal("inherit", _converter.Convert("inherit"));
    }

    [Fact]
    public void ConvertMultipleColorsInValue()
    {
        // A value with two color functions (unlikely but possible)
        var result = _converter.Convert("rgb(255 0 0), rgb(0 0 255)");
        Assert.Equal("#FF0000, #0000FF", result);
    }
}
