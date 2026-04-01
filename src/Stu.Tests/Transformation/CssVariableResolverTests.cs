using Stu.Parsing;
using Stu.Transformation;
using Xunit;

namespace Stu.Tests.Transformation;

public class CssVariableResolverTests
{
    private CssVariableResolver CreateResolverWithTheme(params (string prop, string val)[] vars)
    {
        var resolver = new CssVariableResolver();
        var themeRule = new CssRule(":root, :host",
            vars.Select(v => new CssDeclaration(v.prop, v.val)).ToList());
        var rules = new List<CssRule> { themeRule };
        resolver.ExtractAndRemoveThemeRules(rules);
        return resolver;
    }

    [Fact]
    public void ResolveSpacingCalc()
    {
        var resolver = CreateResolverWithTheme(("--spacing", "4px"));
        Assert.Equal("16px", resolver.Resolve("calc(var(--spacing) * 4)"));
    }

    [Fact]
    public void ResolveSpacingCalcDecimal()
    {
        var resolver = CreateResolverWithTheme(("--spacing", "4px"));
        Assert.Equal("2px", resolver.Resolve("calc(var(--spacing) * 0.5)"));
    }

    [Fact]
    public void ResolveSpacingCalcNegative()
    {
        var resolver = CreateResolverWithTheme(("--spacing", "4px"));
        Assert.Equal("-16px", resolver.Resolve("calc(var(--spacing) * -4)"));
    }

    [Fact]
    public void ResolveSpacingCalcZero()
    {
        var resolver = CreateResolverWithTheme(("--spacing", "4px"));
        Assert.Equal("0", resolver.Resolve("calc(var(--spacing) * 0)"));
    }

    [Fact]
    public void ResolveSimpleVar()
    {
        var resolver = CreateResolverWithTheme(("--color-red-500", "#EF4444"));
        Assert.Equal("#EF4444", resolver.Resolve("var(--color-red-500)"));
    }

    [Fact]
    public void ResolveVarWithFallback()
    {
        var resolver = CreateResolverWithTheme();
        Assert.Equal("16px", resolver.Resolve("var(--unknown, 16px)"));
    }

    [Fact]
    public void ResolveFractionCalc()
    {
        Assert.Equal("50%", new CssVariableResolver().Resolve("calc(1 / 2 * 100%)"));
    }

    [Fact]
    public void ResolveLineHeightCalc()
    {
        // Tailwind v4 uses calc(1.5 / 1) for line-height ratios
        Assert.Equal("1.5", new CssVariableResolver().Resolve("calc(1.5 / 1)"));
    }

    [Fact]
    public void ResolveChainedVars()
    {
        var resolver = CreateResolverWithTheme(
            ("--font-sans", "ui-sans-serif, system-ui, sans-serif"),
            ("--default-font-family", "var(--font-sans)")
        );
        Assert.Equal("ui-sans-serif, system-ui, sans-serif",
            resolver.Resolve("var(--default-font-family)"));
    }

    [Fact]
    public void ExtractRemovesThemeRules()
    {
        var resolver = new CssVariableResolver();
        var rules = new List<CssRule>
        {
            new(":root, :host", new List<CssDeclaration> { new("--spacing", "4px") }),
            new(".p-4", new List<CssDeclaration> { new("padding", "calc(var(--spacing) * 4)") })
        };

        var remaining = resolver.ExtractAndRemoveThemeRules(rules);

        Assert.Single(remaining);
        Assert.Equal(".p-4", remaining[0].Selector);
    }

    [Fact]
    public void PreservesNonVarValues()
    {
        var resolver = new CssVariableResolver();
        Assert.Equal("block", resolver.Resolve("block"));
        Assert.Equal("16px", resolver.Resolve("16px"));
        Assert.Equal("auto", resolver.Resolve("auto"));
    }

    [Fact]
    public void ResolveTextSizeVar()
    {
        var resolver = CreateResolverWithTheme(("--text-lg", "18px"));
        Assert.Equal("18px", resolver.Resolve("var(--text-lg)"));
    }

    [Fact]
    public void ResolveRadiusVar()
    {
        var resolver = CreateResolverWithTheme(("--radius-lg", "8px"));
        Assert.Equal("8px", resolver.Resolve("var(--radius-lg)"));
    }
}
