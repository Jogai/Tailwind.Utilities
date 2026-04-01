using Stu.Parsing;
using Stu.Transformation;
using Xunit;

namespace Stu.Tests.Transformation;

public class LineHeightConverterTests
{
    private readonly LineHeightConverter _converter = new(baseFontSize: 16);

    [Fact]
    public void ConvertPairedLineHeight()
    {
        // text-lg: font-size 18px, line-height 1.5556 → 28px
        var rule = new CssRule(".text-lg", new()
        {
            new("font-size", "18px"),
            new("line-height", "1.5556")
        });

        _converter.ConvertRule(rule);

        Assert.Equal("18px", rule.Declarations[0].Value);
        Assert.Equal("28px", rule.Declarations[1].Value);
    }

    [Fact]
    public void ConvertTextBase()
    {
        // text-base: font-size 16px, line-height 1.5 → 24px
        var rule = new CssRule(".text-base", new()
        {
            new("font-size", "16px"),
            new("line-height", "1.5")
        });

        _converter.ConvertRule(rule);

        Assert.Equal("24px", rule.Declarations[1].Value);
    }

    [Fact]
    public void ConvertText2xl()
    {
        // text-2xl: font-size 24px, line-height 1.3333 → 32px
        var rule = new CssRule(".text-2xl", new()
        {
            new("font-size", "24px"),
            new("line-height", "1.3333")
        });

        _converter.ConvertRule(rule);

        Assert.Equal("32px", rule.Declarations[1].Value);
    }

    [Fact]
    public void ConvertTextXs()
    {
        // text-xs: font-size 12px, line-height 1.3333 → 16px
        var rule = new CssRule(".text-xs", new()
        {
            new("font-size", "12px"),
            new("line-height", "1.3333")
        });

        _converter.ConvertRule(rule);

        Assert.Equal("16px", rule.Declarations[1].Value);
    }

    [Fact]
    public void ConvertStandaloneLeadingUsesBaseFontSize()
    {
        // leading-loose: line-height 2 → 2 * 16 = 32px
        var rule = new CssRule(".leading-loose", new()
        {
            new("line-height", "2")
        });

        _converter.ConvertRule(rule);

        Assert.Equal("32px", rule.Declarations[0].Value);
    }

    [Fact]
    public void ConvertLeadingTight()
    {
        // leading-tight: line-height 1.25 → 1.25 * 16 = 20px
        var rule = new CssRule(".leading-tight", new()
        {
            new("line-height", "1.25")
        });

        _converter.ConvertRule(rule);

        Assert.Equal("20px", rule.Declarations[0].Value);
    }

    [Fact]
    public void ConvertLineHeightOne()
    {
        // text-5xl: font-size 48px, line-height 1 → 48px
        var rule = new CssRule(".text-5xl", new()
        {
            new("font-size", "48px"),
            new("line-height", "1")
        });

        _converter.ConvertRule(rule);

        Assert.Equal("48px", rule.Declarations[1].Value);
    }

    [Fact]
    public void PreserveAlreadyPxLineHeight()
    {
        var rule = new CssRule(".leading-6", new()
        {
            new("line-height", "24px")
        });

        _converter.ConvertRule(rule);

        Assert.Equal("24px", rule.Declarations[0].Value);
    }

    [Fact]
    public void PreserveNormalKeyword()
    {
        var rule = new CssRule(".leading-normal-kw", new()
        {
            new("line-height", "normal")
        });

        _converter.ConvertRule(rule);

        Assert.Equal("normal", rule.Declarations[0].Value);
    }

    [Fact]
    public void DoesNotTouchNonLineHeightProperties()
    {
        var rule = new CssRule(".opacity-50", new()
        {
            new("opacity", "0.5")
        });

        _converter.ConvertRule(rule);

        Assert.Equal("0.5", rule.Declarations[0].Value);
    }

    [Fact]
    public void CustomBaseFontSize()
    {
        var converter = new LineHeightConverter(baseFontSize: 14);

        var rule = new CssRule(".leading-loose", new()
        {
            new("line-height", "2")
        });

        converter.ConvertRule(rule);

        Assert.Equal("28px", rule.Declarations[0].Value);
    }
}
